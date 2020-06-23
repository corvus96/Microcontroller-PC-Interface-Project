
/******************************************************************
                        Proyecto1 Microprocesadores 2: Semaforo
        Este programa Es un semaforo con interrupciones y sensor ultrasónico
                    Elaborado por: Guillermo Raven
                    ARM : STM32F446RE
******************************************************************/

#include "stm32f4xx_hal.h"
#include <stdio.h> 
#include "stm32f4xx_nucleo.h"
#include <string.h>
// Here is a new name more understandable
// Timer work with 16 MHz by default
# define PRESCALER_VALUE  1599; // Timer freq aprox 1KHz
# define PERIOD_VALUE     9; // Interrupt timer is each 1 ms
# define PRESCALER_ULTRA  15; // Timer freq aprox 1MHz
# define PERIOD_ULTRA     1; // Interrupt timer is each 2 us

unsigned short int counter = 1000; // Base of time to 1 s
int state = 1; // 1 = green, 2 = yellow, 3 = red
unsigned short int greenTime = 1000;
unsigned short int yellowTime = 2000;
unsigned short int redTime = 3000;
int buttomDown = 0; // To know when the buttom has been pressed
unsigned int bounce = 120; // bounce Time aprox 120 ms

unsigned short int highLevelPulseTime = 8; // 16 us time to HIGH LEVEL trigger
int pulseMode = 1; // flag to change the state at trigger pulse
unsigned long int lowLevelPulseTime = 250000; // 0.5 s to relax ultrasonic measurement
unsigned long int counter2 = 250000; // To control the trigger period
double highLevelEchoTime = 0; // Time to echo high level
//int echoState = 0; // High level echo flag
float distance; // to save distance 
char buffer[28]; // to transmit by UART

// Structures declarations
static TIM_HandleTypeDef s_TimerInstance = { 
    .Instance = TIM2
};

static TIM_HandleTypeDef ultrasonicTimerInstance = { 
    .Instance = TIM4
};

static GPIO_InitTypeDef GPIO_InitStruct;
static UART_HandleTypeDef s_UARTHandle;

// Call functions before main

void portA_pin_Init(unsigned long int selectPin,unsigned long int mode,unsigned long int pull, unsigned long int speed) {
  __GPIOA_CLK_ENABLE(); // Also you can put here __HAL_RCC_GPIOA_CLK_ENABLE is the same
  GPIO_InitStruct.Pin = selectPin ;
  GPIO_InitStruct.Mode = mode;
  GPIO_InitStruct.Pull = pull;
  GPIO_InitStruct.Speed = speed;
  HAL_GPIO_Init(GPIOA , &GPIO_InitStruct);
}

void uart_Init(){
  __USART2_CLK_ENABLE();
  GPIO_InitStruct.Pin = GPIO_PIN_2;
  GPIO_InitStruct.Mode = GPIO_MODE_AF_PP;
  GPIO_InitStruct.Pull = GPIO_NOPULL;
  GPIO_InitStruct.Alternate = GPIO_AF7_USART2;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_HIGH;
  HAL_GPIO_Init(GPIOA, &GPIO_InitStruct);

  GPIO_InitStruct.Pin = GPIO_PIN_3;
  GPIO_InitStruct.Mode = GPIO_MODE_AF_OD;
  HAL_GPIO_Init(GPIOA, &GPIO_InitStruct);

  s_UARTHandle.Instance = USART2;
  s_UARTHandle.Init.BaudRate = 115200;
  s_UARTHandle.Init.Mode = USART_MODE_TX_RX;
  s_UARTHandle.Init.Parity = USART_PARITY_NONE;
  s_UARTHandle.Init.StopBits = USART_STOPBITS_1;
  s_UARTHandle.Init.WordLength = USART_WORDLENGTH_8B;
  s_UARTHandle.Init.HwFlowCtl  = UART_HWCONTROL_NONE;

  HAL_UART_Init(&s_UARTHandle);
}



// This space correpond with timer control
void timer_Init(){
  __TIM2_CLK_ENABLE(); // Also you can put here __HAL_RCC_TIM2_CLK_ENABLE is the same
  s_TimerInstance.Init.Prescaler = PRESCALER_VALUE ; // ftimer = fclock/(Preescaler + 1)
  s_TimerInstance.Init.CounterMode = TIM_COUNTERMODE_UP;
  s_TimerInstance.Init.Period = PERIOD_VALUE;  //T = (1/ftimer)(period + 1)
  s_TimerInstance.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
  s_TimerInstance.Init.RepetitionCounter = 0;
  HAL_TIM_Base_Init(&s_TimerInstance);
  HAL_TIM_Base_Start_IT(&s_TimerInstance);
}

void ultrasonicTimer_Init(){
  __TIM4_CLK_ENABLE(); // Also you can put here __HAL_RCC_TIM2_CLK_ENABLE is the same
  ultrasonicTimerInstance.Init.Prescaler = PRESCALER_ULTRA ; // ftimer = fclock/(Preescaler + 1)
  ultrasonicTimerInstance.Init.CounterMode = TIM_COUNTERMODE_UP;
  ultrasonicTimerInstance.Init.Period = PERIOD_ULTRA;  //T = (1/ftimer)(period + 1)
  ultrasonicTimerInstance.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
  ultrasonicTimerInstance.Init.RepetitionCounter = 0;
  HAL_TIM_Base_Init(&ultrasonicTimerInstance);
  HAL_TIM_Base_Start_IT(&ultrasonicTimerInstance);
}

void TIM2_IRQHandler(){
  if(__HAL_TIM_GET_FLAG(&s_TimerInstance, TIM_FLAG_UPDATE) != RESET) // Frequency to enter at interrupt aprox 1 KHz
  {
    __HAL_TIM_CLEAR_IT(&s_TimerInstance, TIM_IT_UPDATE);
    if(BSP_PB_GetState(BUTTON_KEY)){
      bounce = 120; // Re-init time to antibounce
      counter--; 
      if(state == 1 && counter == 0){
        HAL_GPIO_WritePin(GPIOA, GPIO_PIN_6,0); // Red OFF
        HAL_GPIO_TogglePin(GPIOA , GPIO_PIN_5); // Green ON
        counter = greenTime;
        state = 2;
      }
      else if (state == 2 && counter == 0)
      {
        HAL_GPIO_WritePin(GPIOA, GPIO_PIN_5,0); // Green OFF
        HAL_GPIO_TogglePin(GPIOA , GPIO_PIN_4); // yellow ON
        counter = yellowTime;
        state = 3;
      } 
      else if (state == 3 && counter == 0){
        HAL_GPIO_WritePin(GPIOA, GPIO_PIN_4,0); // yellow OFF
        HAL_GPIO_TogglePin(GPIOA , GPIO_PIN_6); // Red ON  
        counter = redTime;
        state = 1;
      }
    }else{ // Anti-bounce to the buttom
      bounce--;
      if(bounce==0){
        HAL_GPIO_WritePin(GPIOA, GPIO_PIN_4,0); // yellow OFF
        HAL_GPIO_WritePin(GPIOA, GPIO_PIN_6,0); // Red OFF
        HAL_GPIO_WritePin(GPIOA, GPIO_PIN_5,1); // Green ON
        counter = greenTime;
        state = 2;
      }
    }
  }
}

// void USART2_IRQHandler(){
//   distance = 0; // To reset the distance for a new value
// }

// ***********************************************************

void SysTick_Handler(void) {
  HAL_IncTick();
  HAL_SYSTICK_IRQHandler();
}

// Main program
int main() 
{
  HAL_Init();
  portA_pin_Init(GPIO_PIN_5,GPIO_MODE_OUTPUT_PP,GPIO_PULLUP,GPIO_SPEED_HIGH); // Green LED
  portA_pin_Init(GPIO_PIN_4,GPIO_MODE_OUTPUT_PP,GPIO_PULLUP,GPIO_SPEED_HIGH); // Yellow LED
  portA_pin_Init(GPIO_PIN_6,GPIO_MODE_OUTPUT_PP,GPIO_PULLUP,GPIO_SPEED_HIGH); // Red LED
  portA_pin_Init(GPIO_PIN_9,GPIO_MODE_OUTPUT_PP,GPIO_NOPULL,GPIO_SPEED_HIGH); // trigger
  portA_pin_Init(GPIO_PIN_1,GPIO_MODE_INPUT,GPIO_NOPULL,GPIO_SPEED_HIGH); // Echo
  BSP_PB_Init(BUTTON_KEY, BUTTON_MODE_GPIO);
  uart_Init();
  timer_Init();
  ultrasonicTimer_Init();

  //Interrupts enable
  HAL_NVIC_SetPriority(TIM2_IRQn,0,0); // Set Interrupt priority
  HAL_NVIC_EnableIRQ(TIM2_IRQn);      // Enable interrupt by timer 2

  while (1)
  {
    // Ultrasonic pulse send and read 
    if(__HAL_TIM_GET_FLAG(&ultrasonicTimerInstance, TIM_FLAG_UPDATE) != RESET) // Frequency to enter at interrupt aprox 500 KHz
    {
      __HAL_TIM_CLEAR_IT(&ultrasonicTimerInstance, TIM_IT_UPDATE);
      if(pulseMode == 0){
        counter2--;
        if(counter2 == 0){
          HAL_GPIO_WritePin(GPIOA, GPIO_PIN_9,0); // trigger OFF
          counter2 = lowLevelPulseTime; // 0.5 s counter
          pulseMode = 1;
        } 
      }else{
        counter2--;
        if(counter2 == 0){
          HAL_GPIO_WritePin(GPIOA, GPIO_PIN_9,1); // trigger ON
          counter2 = highLevelPulseTime; // 16 us counter
          pulseMode = 0;
        }
      }
      // When echo is activated, change the flag
      if(HAL_GPIO_ReadPin(GPIOA,GPIO_PIN_1)){
        highLevelEchoTime++;
      }
      if(highLevelEchoTime>0 && HAL_GPIO_ReadPin(GPIOA,GPIO_PIN_1) == 0){
        highLevelEchoTime = highLevelEchoTime - 1;
        distance = (0.00002*highLevelEchoTime*34300)/2; // distance calculus
        char distanceString[32]; // 32 is the Number of bytes in a float
        gcvt(distance, 6, distanceString); // convert float to string with len equal 6
        sprintf(buffer,"The distance is: %s cm\n",distanceString); // joining the data to send
        HAL_UART_Transmit(&s_UARTHandle,buffer, sizeof(buffer), HAL_MAX_DELAY);
        HAL_Delay(100);
        highLevelEchoTime = 0;
      }
    }
  }
}


/******************************************************************
                        Generador de señales
        Este programa permite generar señales de frecuencia
        ajustable del tipo Cuadradas, triangulares y senoidales
        Rango: 2kHz - 100 KHz
                    Elaborado por: Guillermo Raven
******************************************************************/

# include "stm32f4xx_hal.h"
# include "limits.h"
// Here is a new name more understandable


// GLOBAL VARIABLES
static TIM_HandleTypeDef s_TimerInstance = { 
    .Instance = TIM2
};
unsigned int counter = 10000;
static GPIO_InitTypeDef GPIO_InitStruct;
// Call functions before main
void pin_Init();
void timer_Init();

int main(void) {
  HAL_Init();
  pin_Init(GPIO_PIN_5,GPIO_MODE_OUTPUT_PP,GPIO_PULLUP,GPIO_SPEED_HIGH);
  timer_Init();
  HAL_NVIC_SetPriority(TIM2_IRQn,0,0); // Set Interrupt priority
  HAL_NVIC_EnableIRQ(TIM2_IRQn);      // Enable interrupt by timer 2

  while (1)
  {

  }
}

void pin_Init(uint32_t selectPin,uint32_t mode,uint32_t pull, uint32_t speed) {
  __GPIOA_CLK_ENABLE(); // Also you can put here __HAL_RCC_GPIOA_CLK_ENABLE is the same
  GPIO_InitStruct.Pin = selectPin ;
  GPIO_InitStruct.Mode = mode;
  GPIO_InitStruct.Pull = pull;
  GPIO_InitStruct.Speed = speed;
  HAL_GPIO_Init(GPIOA , &GPIO_InitStruct);
}

// This space correpond with timer control
void timer_Init(){
  __TIM2_CLK_ENABLE(); // Also you can put here __HAL_RCC_TIM2_CLK_ENABLE is the same
  s_TimerInstance.Init.Prescaler = 60000;
  s_TimerInstance.Init.CounterMode = TIM_COUNTERMODE_UP;
  s_TimerInstance.Init.Period = 0;
  s_TimerInstance.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
  s_TimerInstance.Init.RepetitionCounter = 0;
  HAL_TIM_Base_Init(&s_TimerInstance);
  HAL_TIM_Base_Start(&s_TimerInstance);
}

void TIM2_IRQHandler(){
  HAL_TIM_IRQHandler(&s_TimerInstance);
}

void HAL_TIM_PeriodElapsedCallback(TIM_HandleTypeDef *htim)
{
  counter --;
  if(counter == 0){
    HAL_GPIO_TogglePin(GPIOA , GPIO_PIN_5);
    counter = 10000;
  }
}
// ***********************************************************

void SysTick_Handler(void) {
  HAL_IncTick();
  HAL_SYSTICK_IRQHandler();
}
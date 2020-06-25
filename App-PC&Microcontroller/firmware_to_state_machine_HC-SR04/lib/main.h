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
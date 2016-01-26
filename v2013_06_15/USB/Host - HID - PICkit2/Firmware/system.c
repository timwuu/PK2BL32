#include <xc.h>
#include <uart2.h>
#include "HardwareProfile.h"
#include "timer_1ms.h"
#include "system.h"

// PIC32MX250F128B Configuration Bit Settings

// 'C' source line config statements

// DEVCFG3
// USERID = No Setting
#pragma config PMDL1WAY = OFF           // Peripheral Module Disable Configuration (Allow multiple reconfigurations)
#pragma config IOL1WAY = OFF            // Peripheral Pin Select Configuration (Allow multiple reconfigurations)
#pragma config FUSBIDIO = OFF           // USB USID Selection (Controlled by Port Function)
#pragma config FVBUSONIO = ON           // USB VBUS ON Selection (Controlled by USB Module)

// DEVCFG2
#pragma config FPLLIDIV = DIV_5         // PLL Input Divider (5x Divider)
#pragma config FPLLMUL = MUL_24         // PLL Multiplier (24x Multiplier)
#pragma config UPLLIDIV = DIV_5         // USB PLL Input Divider (5x Divider)
#pragma config UPLLEN = ON              // USB PLL Enable (Enabled)
#pragma config FPLLODIV = DIV_2         // System PLL Output Clock Divider (PLL Divide by 2)

// DEVCFG1
#pragma config FNOSC = PRIPLL           // Oscillator Selection Bits (Primary Osc w/PLL (XT+,HS+,EC+PLL))
#pragma config FSOSCEN = OFF            // Secondary Oscillator Enable (Disabled)
#pragma config IESO = OFF               // Internal/External Switch Over (Disabled)
#pragma config POSCMOD = HS             // Primary Oscillator Configuration (HS osc mode)
#pragma config OSCIOFNC = OFF           // CLKO Output Signal Active on the OSCO Pin (Disabled)
#pragma config FPBDIV = DIV_1           // Peripheral Clock Divisor (Pb_Clk is Sys_Clk/1)
#pragma config FCKSM = CSDCMD           // Clock Switching and Monitor Selection (Clock Switch Disable, FSCM Disabled)
#pragma config WDTPS = PS1              // Watchdog Timer Postscaler (1:1)
#pragma config WINDIS = OFF             // Watchdog Timer Window Enable (Watchdog Timer is in Non-Window Mode)
#pragma config FWDTEN = OFF             // Watchdog Timer Enable (WDT Disabled (SWDTEN Bit Controls))
#pragma config FWDTWINSZ = WINSZ_25     // Watchdog Timer Window Size (Window Size is 25%)

// DEVCFG0
#pragma config JTAGEN = OFF             // JTAG Enable (JTAG Disabled)
#pragma config ICESEL = ICS_PGx3        // ICE/ICD Comm Channel Select (Communicate on PGEC3/PGED3)
#pragma config PWP = OFF                // Program Flash Write Protect (Disable)
#pragma config BWP = OFF                // Boot Flash Write Protect bit (Protection Disabled)
#pragma config CP = OFF                 // Code Protect (Protection Disabled)

/*********************************************************************
* Function: void SYSTEM_Initialize( SYSTEM_STATE state )
*
* Overview: Initializes the system.
*
* PreCondition: None
*
* Input:  SYSTEM_STATE - the state to initialize the system into
*
* Output: None
*
********************************************************************/
void SYSTEM_Initialize( SYSTEM_STATE state )
{
    switch(state)
    {
        case SYSTEM_STATE_USB_HOST:
            //PRINT_SetConfiguration(PRINT_CONFIGURATION_UART);
            //UART2Init();  //?timijk for DEBUGGING
            
            break;
            
        case SYSTEM_STATE_USB_HOST_HID_PICKIT:
            LED_Enable(LED_USB_HOST_HID_PICKIT_DEVICE_READY);

            //also setup UART here
            //PRINT_SetConfiguration(PRINT_CONFIGURATION_UART);
            //timijk 2015.04.11 LCD_CursorEnable(true);
            UART2Init();
            IEC1bits.U2RXIE= 1;  //enable RXIE
            
            TIMER_SetConfiguration(TIMER_CONFIGURATION_1MS);

            break;
    }
}

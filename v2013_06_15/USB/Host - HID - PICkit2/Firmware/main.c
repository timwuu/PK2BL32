/********************************************************************
 Software License Agreement:

 The software supplied herewith by Microchip Technology Incorporated
 (the "Company") for its PIC(R) Microcontroller is intended and
 supplied to you, the Company's customer, for use solely and
 exclusively on Microchip PIC Microcontroller products. The
 software is owned by the Company and/or its supplier, and is
 protected under applicable copyright laws. All rights are reserved.
 Any use in violation of the foregoing restrictions may subject the
 user to criminal sanctions under applicable laws, as well as to
 civil liability for the breach of the terms and conditions of this
 license.

 THIS SOFTWARE IS PROVIDED IN AN "AS IS" CONDITION. NO WARRANTIES,
 WHETHER EXPRESS, IMPLIED OR STATUTORY, INCLUDING, BUT NOT LIMITED
 TO, IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
 PARTICULAR PURPOSE APPLY TO THIS SOFTWARE. THE COMPANY SHALL NOT,
 IN ANY CIRCUMSTANCES, BE LIABLE FOR SPECIAL, INCIDENTAL OR
 CONSEQUENTIAL DAMAGES, FOR ANY REASON WHATSOEVER.
 *******************************************************************/

/** INCLUDES *******************************************************/
#include <xc.h>
#include <stdlib.h>
#include <usb/usb.h>
#include <usb/usb_host_hid.h>

//#include <system.h>
#include "app_host_hid_bridge.h"
#include <uart_buffer.h>

#include "system.h"
#include "HardwareProfile.h"
#include "timer_1ms.h"

void setup();
void loop();
void ConfigurePinReMap();

//void loop()
//{
//    int i;
//    int j;
//    
//    for( i=0; i<5000; i++)
//        for( j=0; j<1000; j++);
//        
//    LATBbits.LATB4 ^= 1;
//}
//
//void blinkLED()
//{
//    char c;
//    
//    if(IFS1bits.U2RXIF) { U2TXREG = '-'; c=U2RXREG; IFS1bits.U2RXIF=0;}
//    else U2TXREG ='@';
//    LATBbits.LATB3 ^= 1;
//
//}

/********************************************************************
 * Function:        void main(void)
 *
 * PreCondition:    None
 *
 * Input:           None
 *
 * Output:          None
 *
 * Side Effects:    None
 *
 * Overview:        Main program entry point.
 *
 * Note:            None
 *******************************************************************/
void main(void) {
    
    // test command buffer
//    if(0)
//    {
//    UB_Init();
//    
//    UB_ProcessCommand( STX);
//    UB_ProcessCommand( STX);
//    UB_ProcessCommand( 0x76);
//    UB_ProcessCommand( 0x8A);
//    UB_ProcessCommand( ETX);
//    
//    UB_ProcessCommand( STX);
//    UB_ProcessCommand( STX);
//    UB_ProcessCommand( 0);
//    UB_ProcessCommand( ETX);
//    
//    APP_HostHIDPICkitTasks();
//    
//    UB_ProcessCommand( STX);
//    UB_ProcessCommand( STX);
//    UB_ProcessCommand( 0);
//    UB_ProcessCommand( ETX);
//    }
    
    //------------- test UART ----------------
    setup();
   
//    if(0)
//    {
//       //test Timer_1ms
//       TIMER_SetConfiguration ( TIMER_CONFIGURATION_1MS );
//       TIMER_RequestTick( blinkLED, 500); //500ms
//       
//       //test UART2
//       IEC1bits.U2RXIE= 1;  //enable RXIE
//
//    }
    
    //--------------------------------
    SYSTEM_Initialize(SYSTEM_STATE_USB_HOST);

    //Initialize the stack
    USBHostInit(0);

    APP_HostHIDPICkitInitialize();

    UB_Init(); // UART_Buffer

    while (1) {
        USBHostTasks();
        USBHostHIDTasks();

        //Application specific tasks
        APP_HostHIDPICkitTasks();

        UB_TxTasks();

        LATBbits.LATB3 ^= 1;
    }//end while
}//end main

/****************************************************************************
  Function:
    bool USB_ApplicationEventHandler( uint8_t address, USB_EVENT event,
                void *data, uint32_t size )

  Summary:
    This is the application event handler.  It is called when the stack has
    an event that needs to be handled by the application layer rather than
    by the client driver.

  Description:
    This is the application event handler.  It is called when the stack has
    an event that needs to be handled by the application layer rather than
    by the client driver.  If the application is able to handle the event, it
    returns true.  Otherwise, it returns false.

  Precondition:
    None

  Parameters:
    uint8_t address    - Address of device where event occurred
    USB_EVENT event - Identifies the event that occured
    void *data      - Pointer to event-specific data
    uint32_t size      - Size of the event-specific data

  Return Values:
    true    - The event was handled
    false   - The event was not handled

  Remarks:
    The application may also implement an event handling routine if it
    requires knowledge of events.  To do so, it must implement a routine that
    matches this function signature and define the USB_HOST_APP_EVENT_HANDLER
    macro as the name of that function.
 ***************************************************************************/
//******************************************************************************
//******************************************************************************
// USB Support Functions
//******************************************************************************
//******************************************************************************

BOOL USB_ApplicationEventHandler( BYTE address, USB_EVENT event, void *data, DWORD size ) {
    switch ((INT) event) {
        case EVENT_VBUS_REQUEST_POWER:
        case EVENT_VBUS_RELEASE_POWER:
        case EVENT_HUB_ATTACH:
        case EVENT_UNSUPPORTED_DEVICE:
        case EVENT_CANNOT_ENUMERATE:
        case EVENT_CLIENT_INIT_ERROR:
        case EVENT_OUT_OF_MEMORY:
        case EVENT_UNSPECIFIED_ERROR: // This should never be generated.
            return TRUE;
            break;

        case EVENT_HID_RPT_DESC_PARSED:
#ifdef APPL_COLLECT_PARSED_DATA
            return (APP_HostHIDPICkitReportParser());
#else
            return TRUE;
#endif
            break;

        default:
            break;
    }
    return FALSE;
}

void setup() {

    int value;

    value = SYSTEMConfigWaitStatesAndPB(GetSystemClock());

    // Enable the cache for the best performance
    CheKseg0CacheOn();

    INTEnableSystemMultiVectoredInt();

    value = OSCCON;
    while (!(value & 0x00000020)) {
        value = OSCCON; // Wait for PLL lock to stabilize
    }

    LATB=0;

    ANSELA = 0x00;
    ANSELB = 0x00;
    TRISA = 0xFFFF;
    TRISB = 0xFFFF;

    TRISBbits.TRISB1= OUTPUT_PIN;
    TRISBbits.TRISB2= OUTPUT_PIN;
    TRISBbits.TRISB3= OUTPUT_PIN;

    //?timijk debug
    //TRISBbits.TRISB4= OUTPUT_PIN;
    
    ConfigurePinReMap();
    
}

void ConfigurePinReMap()
{
    //setup UART2 RX/TX pins
    SYSKEY = 0x00;
    SYSKEY = 0xAA996655;
    SYSKEY = 0x556699AA;
    
    CFGCONbits.IOLOCK=0;
    U2RXR= 0x04;    // U2Rx:RPB8
    RPB9Rbits.RPB9R= 0x02;    // U2Tx:RPB9
    CFGCONbits.IOLOCK=1;
    
    SYSKEY = 0x00;
  
    return;
    
}

void __ISR(_UART2_VECTOR, ipl6AUTO) _U2Interrupt(void)
{
    unsigned char c;

    if (IFS1bits.U2RXIF) {
        
        LATBbits.LATB1 ^= 1;
        
        while (U2STAbits.URXDA) {
            c = U2RXREG;
            UB_ProcessCommand(c);
        }

        IFS1bits.U2RXIF = 0;
    }
}
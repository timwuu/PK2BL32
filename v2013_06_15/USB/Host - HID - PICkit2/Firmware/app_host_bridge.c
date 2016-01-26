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
#include <usb/usb.h>
#include <usb/usb_host_hid.h>

#include <stdint.h>
#include <stdbool.h>
#include <system.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>

#include <timer_1ms.h>
#include <uart_buffer.h>
#include "HardwareProfile.h"
//#include <uart.h>

// *****************************************************************************
// *****************************************************************************
// Type definitions
// *****************************************************************************
// *****************************************************************************

typedef enum _APP_STATE {
    DEVICE_NOT_CONNECTED,
    WAITING_FOR_DEVICE,
    DEVICE_CONNECTED, /* Device Enumerated  - Report Descriptor Parsed */
    GET_INPUT_REPORT, /* perform operation on received report */
    INPUT_REPORT_PENDING,
    SEND_OUTPUT_REPORT, /* Not needed in case of mouse */
    OUTPUT_REPORT_PENDING,
    ERROR_REPORTED
} PICKIT_STATE;

typedef struct {
    PICKIT_STATE state;
    bool goSEND_OUTPUT_REPORT;  //timijk 2016.01.11    
    bool inUse;

    uint16_t size;
    uint8_t *buffer;

    HID_DATA_DETAILS details;

} PICKIT;

#define MAX_ERROR_COUNTER               (200)

// *****************************************************************************
// *****************************************************************************
// Local Variables
// *****************************************************************************
// *****************************************************************************

int retry_count;

static PICKIT pickit;

//extern unsigned char cmdBuffer[];
extern unsigned char rspBuffer[];

// *****************************************************************************
// *****************************************************************************
// Local Function Prototypes
// *****************************************************************************
// *****************************************************************************
static void App_ProcessInputReport(void);

static void App_PrepareOutputReport(void);

// *****************************************************************************
// *****************************************************************************
// Functions
// *****************************************************************************
// *****************************************************************************

/*********************************************************************
 * Function: void APP_HostHIDTimerHandler(void);
 *
 * Overview: Switches over the state machine state to get a new report
 *           periodically if the device is idle
 *
 * PreCondition: None
 *
 * Input: None
 *
 * Output: None
 *
 ********************************************************************/
static void APP_HostHIDTimerHandler(void) {
}

/*********************************************************************
 * Function: void APP_HostHIDPICkitInitialize(void);
 *
 * Overview: Initializes the demo code
 *
 * PreCondition: None
 *
 * Input: None
 *
 * Output: None
 *
 ********************************************************************/
void APP_HostHIDPICkitInitialize() {
    pickit.state = DEVICE_NOT_CONNECTED;
    pickit.goSEND_OUTPUT_REPORT = false;
    pickit.inUse = false;
    pickit.buffer = NULL;
    
    // init DMA
    // Iniialisation DMA UART TX
    IFS1bits.U2TXIF = 0; 
    IEC1bits.U2TXIE = 0;

    IEC1CLR= _IEC1_DMA0IE_MASK; // disable DMA channel 0 interrupts
    IFS1CLR= _IFS1_DMA0IF_MASK;  //0x00010000; // clear existing DMA channel 0 interrupt flag
    DMACONSET= _DMACON_ON_MASK; // enable the DMA controller

    // program the transfer
    DCH0SSA=KVA_TO_PA(rspBuffer); // transfer source physical address
    DCH0DSA=KVA_TO_PA(&U2TXREG); // transfer destination physical address
    DCH0ECONbits.CHSIRQ = _UART2_TX_IRQ; // IRQ UART2 TX
    DCH0ECONbits.SIRQEN = 1; // Activation de la commande via interruption
    DCH0SSIZ=BUFFER_SIZE; // source size 256 bytes
    DCH0DSIZ=1; // destination size 1 bytes
    DCH0CSIZ=1; // 1 bytes transferred per event    
}

/*********************************************************************
 * Function: void APP_HostHIDPICkitTasks(void);
 *
 * Overview: Keeps the demo running.
 *
 * PreCondition: The demo should have been initialized via
 *   the APP_HostHIDPICkitInitialize()
 *
 * Input: None
 *
 * Output: None
 *
 ********************************************************************/
void APP_HostHIDPICkitTasks() {
    uint8_t error;
    uint8_t count;

    
    
    if (!USBHostHID_ApiDeviceDetect()) {
        if (pickit.state != WAITING_FOR_DEVICE) {
            pickit.state = DEVICE_NOT_CONNECTED;
            
#ifdef DEBUG_MODE
            UART2PrintString("APP: PICkit Disconnected!\n");
#endif
            pickit.inUse = false;

            if (pickit.buffer != NULL) {
                free(pickit.buffer);
                pickit.buffer = NULL;
            }
        }
    }
       
    switch (pickit.state) {
        case DEVICE_NOT_CONNECTED:
            //PRINT_ClearScreen();
#ifdef DEBUG_MODE
            UART2PrintString("APP: Attach PICkit\n");
#endif            
            pickit.state = WAITING_FOR_DEVICE;
            IEC1bits.U2RXIE= 0;  //?timijk disable RXIE
            LED_Off(LED_USB_HOST_HID_PICKIT_DEVICE_READY);
            break;

        case WAITING_FOR_DEVICE:
            if (USBHostHID_ApiDeviceDetect()) /* True if report descriptor is parsed with no error */ {
                
                SYSTEM_Initialize(SYSTEM_STATE_USB_HOST_HID_PICKIT);
                
                LED_On(LED_USB_HOST_HID_PICKIT_DEVICE_READY);
                      
                pickit.state = DEVICE_CONNECTED;
                                                    
            }
            break;

        case DEVICE_CONNECTED:
            if( isCmdQueEmpty()) break;
            
            if(getCmdQueSize()==1)
            {   //if the size of commands is 1, it means read from USB device
                pickit.state = GET_INPUT_REPORT;
                
                incCmdBufferRdIdx();
            }
            else
            {
                pickit.state = SEND_OUTPUT_REPORT;                
            }
            break;

        case GET_INPUT_REPORT:
                       
            if (USBHostHID_ApiGetReport(pickit.details.reportID,
                    pickit.details.interfaceNum,
                    pickit.size,
                    (uint8_t *)rspBuffer //pickit.buffer
                    )
                    ) {
                /* Host may be busy/error -- keep trying */
            } else {
                pickit.state = INPUT_REPORT_PENDING;
            }
            break;

        case INPUT_REPORT_PENDING:
            if (USBHostHID_ApiTransferIsComplete(&error, &count)) {
                if (error || (count == 0)) {
                   pickit.state = GET_INPUT_REPORT; // retry           
                } else {
                    
                    pickit.state = DEVICE_CONNECTED;

                    App_ProcessInputReport();
                }
            }
            break;

        case SEND_OUTPUT_REPORT:      
            //TMR4=0;   //?timijk performance test::start timer
            App_PrepareOutputReport();
            if (USBHostHID_ApiSendReport(pickit.details.reportID,
                    pickit.details.interfaceNum,
                    pickit.size,
                    pickit.buffer )) {
                /* Host may be busy/error -- keep trying */
            } else {
                
                pickit.state = OUTPUT_REPORT_PENDING;
                //retry_count=0;
            }
            break;

        case OUTPUT_REPORT_PENDING:
            //timijk 2016.01.13 Issue
            if (USBHostHID_ApiTransferIsComplete(&error, &count)) {
                //signal the transaction is complete 
                U2TXREG = TMR4>>8;
                pickit.state = DEVICE_CONNECTED;
                //LATBbits.LATB4=0;
            }
            break;

        case ERROR_REPORTED:
            break;

        default:
            break;

    }
}

/****************************************************************************
  Function:
    BOOL USB_HID_DataCollectionHandler(void)
  Description:
    This function is invoked by HID client , purpose is to collect the
    details extracted from the report descriptor. HID client will store
    information extracted from the report descriptor in data structures.
    Application needs to create object for each report type it needs to
    extract.
    For ex: HID_DATA_DETAILS pickit.keys.modifier.details;
    HID_DATA_DETAILS is defined in file usb_host_hid_appl_interface.h
    Each member of the structure must be initialized inside this function.
    Application interface layer provides functions :
    USBHostHID_ApiFindBit()
    USBHostHID_ApiFindValue()
    These functions can be used to fill in the details as shown in the demo
    code.

  Precondition:
    None

  Parameters:
    None

  Return Values:
    true    - If the report details are collected successfully.
    false   - If the application does not find the the supported format.

  Remarks:
    This Function name should be entered in the USB configuration tool
    in the field "Parsed Data Collection handler".
    If the application does not define this function , then HID cient
    assumes that Application is aware of report format of the attached
    device.
 ***************************************************************************/
bool APP_HostHIDPICkitReportParser(void) {
    uint8_t NumOfReportItem = 0;
    uint8_t i;
    USB_HID_ITEM_LIST* pitemListPtrs;
    USB_HID_DEVICE_RPT_INFO* pDeviceRptinfo;
    HID_REPORTITEM *reportItem;
    HID_USAGEITEM *hidUsageItem;
    //uint8_t usageIndex;
   // uint8_t reportIndex;
   // uint8_t usageItem;
    
    /* The pickit is already in use. */
    if (pickit.inUse == true) {

        //PRINT_String( "-In Use-\r\n",10);
        return false;
    }

    pDeviceRptinfo = USBHostHID_GetCurrentReportInfo(); // Get current Report Info pointer
    pitemListPtrs = USBHostHID_GetItemListPointers(); // Get pointer to list of item pointers

    /* Find Report Item Index for Modifier Keys */
    /* Once report Item is located , extract information from data structures provided by the parser */
    NumOfReportItem = pDeviceRptinfo->reportItems;

#ifdef DEBUG_MODE
    UART2PrintString("APP: NumOfReportItem:");
    UART2PutHex(NumOfReportItem);
    UART2PutChar('\n');

    USBHID_ReportDecriptor_Dump();
#endif
    
    reportItem = &pitemListPtrs->reportItemList[0];
    
    pickit.details.reportLength = 64; // (pitemListPtrs->reportList[reportIndex].inputBits + 7) / 8;
    pickit.details.reportID = (uint8_t) reportItem->globals.reportID;
    pickit.details.bitOffset = (uint8_t) reportItem->startBit;
    pickit.details.bitLength = (uint8_t) reportItem->globals.reportsize;
    pickit.details.count = (uint8_t) reportItem->globals.reportCount;
    pickit.details.interfaceNum = USBHostHID_ApiGetCurrentInterfaceNum();

    pickit.size = 64;
    pickit.buffer = (uint8_t*) malloc(pickit.size);
    pickit.inUse = true;

    return (pickit.inUse);
}

/****************************************************************************
  Function:
    void App_ProcessInputReport(void)

  Description:
    This function processes input report received from HID device.

  Precondition:
    None

  Parameters:
    None

  Return Values:
    None

  Remarks:
    None
 ***************************************************************************/
static void App_ProcessInputReport(void) {
    //UB_PrepareResponseBuffer( pickit.buffer);
    
    DCH0INTCLR=0x00ff00ff; // clear existing events, disable all interrupts
    DCH0CONbits.CHEN = 1;
}

//void UB_PrepareOutputReport( uint8_t *buffer);

static void App_PrepareOutputReport(void) {
    UB_PrepareOutputReport( pickit.buffer);
}

//void App_SetReportState(uint8_t io) {
//
//    if (io) {
//        if (pickit.state == DEVICE_CONNECTED) pickit.state = GET_INPUT_REPORT;
//    } else {
//        if (pickit.state == DEVICE_CONNECTED) pickit.state = SEND_OUTPUT_REPORT;
//    }            
//  
//}
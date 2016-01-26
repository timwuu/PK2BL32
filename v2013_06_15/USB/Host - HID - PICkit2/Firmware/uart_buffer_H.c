#include <stdbool.h>
#include <uart_buffer.h>

#define QUE_SIZE 8

typedef enum _BUFFER_STATE {
    EMPTY,
    CMD_TX,        
    CMD_TX_DLE,
    TX_COMPLETE,
    NON_EMPTY        
} BUFFER_STATE;

unsigned char cmdQue[QUE_SIZE][BUFFER_SIZE];
unsigned int cmdQueWrIdx;
unsigned int cmdQueRdIdx;

unsigned int cmdQueSize[QUE_SIZE];

unsigned char *cmdBuffer; 
unsigned char rspBuffer[BUFFER_SIZE];

unsigned int cmdBufferIndex;
unsigned int rspBufferIndex;

unsigned char cmdBufferChecksum;

BUFFER_STATE cmdBufferState;
BUFFER_STATE rspBufferState;

unsigned char *getCmdBufferWr()
{
    return (unsigned char *)&cmdQue[cmdQueWrIdx];
}

void setCmdQueSize( unsigned int i)
{
    cmdQueSize[cmdQueWrIdx]=i;
}


void incCmdBufferWrIdx()
{
    cmdQueWrIdx= (cmdQueWrIdx+1)%QUE_SIZE;
}

unsigned char *getCmdBufferRd()
{
    return (unsigned char *)&cmdQue[cmdQueRdIdx];
}

unsigned int getCmdQueSize()
{
    return cmdQueSize[cmdQueRdIdx];
}

void incCmdBufferRdIdx()
{
    cmdQueRdIdx= (cmdQueRdIdx+1)%QUE_SIZE;
}

bool isCmdQueEmpty()
{
    if (cmdQueRdIdx==cmdQueWrIdx) return true;
    return false;
}


void UB_Init(void)
{
    cmdBufferState=EMPTY;  
    cmdBufferIndex=0;
    rspBufferIndex=0;
    
    cmdQueWrIdx=0;
    cmdQueRdIdx=0;
}

void UB_SetCmdBufferStateEmpty()
{
    cmdBufferState= EMPTY;
}

void UB_ProcessCommand( unsigned char rxByte)
{
    switch(cmdBufferState)
    {
        case EMPTY:
            if(rxByte==STX) {
                TMR4 = 0; //?timijk performance test::start timer
                cmdBuffer = getCmdBufferWr();
                cmdBufferState = CMD_TX;
            }
            break;
        case CMD_TX:
            switch (rxByte) {
                case STX: //Start over if STX
                    cmdBufferIndex=0;
                    cmdBufferChecksum = 0;
                    break;
                case ETX: //End of packet if ETX
                    cmdBufferChecksum = ~cmdBufferChecksum + 1; //test checksum
                    if (cmdBufferChecksum == 0) {
                         
                        cmdBufferState = EMPTY;                       
                        setCmdQueSize(cmdBufferIndex);
                        incCmdBufferWrIdx();
                                               
                        return; //return if OK
                    }
               
                    cmdBufferState = EMPTY; //otherwise restart                       
                    cmdBufferIndex = 0x00;
                    break;

                case DLE: //If DLE, treat next as data
                    cmdBufferState= CMD_TX_DLE;
                    break;
                default: //get data, put in buffer
                    cmdBufferChecksum += rxByte;
                    cmdBuffer[cmdBufferIndex++]= rxByte;
                    break;
            }
            break;
        case CMD_TX_DLE:
            cmdBufferChecksum += rxByte;
            cmdBuffer[cmdBufferIndex++] = rxByte;
            cmdBufferState= CMD_TX;
            break;
        default:
            break;
    }
    
}

void UB_PrepareOutputReport( uint8_t *buffer) {
    unsigned int i;
    unsigned int size;
    unsigned char *pt;

    size = getCmdQueSize() - 1;
    pt = getCmdBufferRd();

    for (i = 0; i < size; i++) buffer[i] = pt[i];
    for (; i < 64; i++) buffer[i] = 0xAD; //'Z':End of Buffer    
    
    incCmdBufferRdIdx();
}

void UB_PrepareResponseBuffer( uint8_t *buffer)
{
    int i;
    for( i=0; i<BUFFER_SIZE; i++) rspBuffer[i]= buffer[i];
    rspBufferIndex = BUFFER_SIZE;
    rspBufferState = NON_EMPTY;
}

void UB_TxTasks(void) {
    
    if( !IFS1bits.DMA0IF) return;
    IFS1bits.DMA0IF=0;
    rspBufferState = EMPTY;
    return;

    //send data if the UTXBF is not full
//    if ((rspBufferIndex > 0) && (U2STAbits.UTXBF == 0)) {
//        U2TXREG = rspBuffer[BUFFER_SIZE-rspBufferIndex];
//        rspBufferIndex--;
//        //Nop();
//        //Nop();
//    }
//
//    if (rspBufferIndex == 0) rspBufferState = EMPTY;
}
# PK2BL32
an adapter enables PICkit 2 with wireless programming ability

---------------------------
2016.01.26

v2013_06_15: the package created by MPLAB X 3.15
   - Microchip MLA v2013-06-15
   - PIC32MX250F128B 40/50MHz
   - X'tal: 20MHz, Fosc: 48MHz
   - Baud Rate 921,600

PK2BL32_pcb.png: the PCB Layout (Preliminary)
   - X'tal: 20MHz
   - LED1.H: PIC32 running
   - LED2.H: USB device attached 
   - LED3.H/L: PIC32 is receiving commands from PICkit2V2 
   - HC-06 pinouts: VCC-GND-TX-RX

PICkit2V2S1: modified version to use COM Port to talk to PICkit 2
   - HC-06 COM Port: COM4, defined in BT.cs
   - Use "Release" mode to run app
   - CmdLog.txt, CmdLogScript.txt: logs of commands

This version can also support the following PICs by swapping the PK2DeviceFile.xml file.
   - some of PIC32MXs
   - dsPIC33EP256MC202
   - PIC16F1459
   - PIC18LF25K50

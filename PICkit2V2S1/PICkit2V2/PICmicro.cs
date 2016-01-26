﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pk2 = PICkit2V2.PICkitFunctions;
using KONST = PICkit2V2.Constants;

namespace PICkit2V2
{
    public class PICmicro
    {
        public static DelegateStatusWin UpdateStatusWinText;
        public static DelegateResetStatusBar ResetStatusBar;
        public static DelegateStepStatusBar StepStatusBar;

        public int DeviceID = 0;

        protected static byte ICSPSpeedRestore = 0;
        protected static bool PEGoodOnWrite = false;

        protected int PE_Version;
        protected int PE_ID;
        protected uint[] PE_Code;

        protected static byte[] BitReverseTable = new byte[256]
         {
          0x00, 0x80, 0x40, 0xC0, 0x20, 0xA0, 0x60, 0xE0, 0x10, 0x90, 0x50, 0xD0, 0x30, 0xB0, 0x70, 0xF0, 
          0x08, 0x88, 0x48, 0xC8, 0x28, 0xA8, 0x68, 0xE8, 0x18, 0x98, 0x58, 0xD8, 0x38, 0xB8, 0x78, 0xF8, 
          0x04, 0x84, 0x44, 0xC4, 0x24, 0xA4, 0x64, 0xE4, 0x14, 0x94, 0x54, 0xD4, 0x34, 0xB4, 0x74, 0xF4, 
          0x0C, 0x8C, 0x4C, 0xCC, 0x2C, 0xAC, 0x6C, 0xEC, 0x1C, 0x9C, 0x5C, 0xDC, 0x3C, 0xBC, 0x7C, 0xFC, 
          0x02, 0x82, 0x42, 0xC2, 0x22, 0xA2, 0x62, 0xE2, 0x12, 0x92, 0x52, 0xD2, 0x32, 0xB2, 0x72, 0xF2, 
          0x0A, 0x8A, 0x4A, 0xCA, 0x2A, 0xAA, 0x6A, 0xEA, 0x1A, 0x9A, 0x5A, 0xDA, 0x3A, 0xBA, 0x7A, 0xFA,
          0x06, 0x86, 0x46, 0xC6, 0x26, 0xA6, 0x66, 0xE6, 0x16, 0x96, 0x56, 0xD6, 0x36, 0xB6, 0x76, 0xF6, 
          0x0E, 0x8E, 0x4E, 0xCE, 0x2E, 0xAE, 0x6E, 0xEE, 0x1E, 0x9E, 0x5E, 0xDE, 0x3E, 0xBE, 0x7E, 0xFE,
          0x01, 0x81, 0x41, 0xC1, 0x21, 0xA1, 0x61, 0xE1, 0x11, 0x91, 0x51, 0xD1, 0x31, 0xB1, 0x71, 0xF1,
          0x09, 0x89, 0x49, 0xC9, 0x29, 0xA9, 0x69, 0xE9, 0x19, 0x99, 0x59, 0xD9, 0x39, 0xB9, 0x79, 0xF9, 
          0x05, 0x85, 0x45, 0xC5, 0x25, 0xA5, 0x65, 0xE5, 0x15, 0x95, 0x55, 0xD5, 0x35, 0xB5, 0x75, 0xF5,
          0x0D, 0x8D, 0x4D, 0xCD, 0x2D, 0xAD, 0x6D, 0xED, 0x1D, 0x9D, 0x5D, 0xDD, 0x3D, 0xBD, 0x7D, 0xFD,
          0x03, 0x83, 0x43, 0xC3, 0x23, 0xA3, 0x63, 0xE3, 0x13, 0x93, 0x53, 0xD3, 0x33, 0xB3, 0x73, 0xF3, 
          0x0B, 0x8B, 0x4B, 0xCB, 0x2B, 0xAB, 0x6B, 0xEB, 0x1B, 0x9B, 0x5B, 0xDB, 0x3B, 0xBB, 0x7B, 0xFB,
          0x07, 0x87, 0x47, 0xC7, 0x27, 0xA7, 0x67, 0xE7, 0x17, 0x97, 0x57, 0xD7, 0x37, 0xB7, 0x77, 0xF7, 
          0x0F, 0x8F, 0x4F, 0xCF, 0x2F, 0xAF, 0x6F, 0xEF, 0x1F, 0x9F, 0x5F, 0xDF, 0x3F, 0xBF, 0x7F, 0xFF
        };

        protected static ushort[] CRC_LUT_Array = new ushort[256]
                                         {  0x0000,0x1021,0x2042,0x3063,0x4084,0x50a5,0x60c6,0x70e7,
                                            0x8108,0x9129,0xa14a,0xb16b,0xc18c,0xd1ad,0xe1ce,0xf1ef,
                                            0x1231,0x0210,0x3273,0x2252,0x52b5,0x4294,0x72f7,0x62d6,
                                            0x9339,0x8318,0xb37b,0xa35a,0xd3bd,0xc39c,0xf3ff,0xe3de,
                                            0x2462,0x3443,0x0420,0x1401,0x64e6,0x74c7,0x44a4,0x5485,
                                            0xa56a,0xb54b,0x8528,0x9509,0xe5ee,0xf5cf,0xc5ac,0xd58d,
                                            0x3653,0x2672,0x1611,0x0630,0x76d7,0x66f6,0x5695,0x46b4,
                                            0xb75b,0xa77a,0x9719,0x8738,0xf7df,0xe7fe,0xd79d,0xc7bc,
                                            0x48c4,0x58e5,0x6886,0x78a7,0x0840,0x1861,0x2802,0x3823,
                                            0xc9cc,0xd9ed,0xe98e,0xf9af,0x8948,0x9969,0xa90a,0xb92b,
                                            0x5af5,0x4ad4,0x7ab7,0x6a96,0x1a71,0x0a50,0x3a33,0x2a12,
                                            0xdbfd,0xcbdc,0xfbbf,0xeb9e,0x9b79,0x8b58,0xbb3b,0xab1a,
                                            0x6ca6,0x7c87,0x4ce4,0x5cc5,0x2c22,0x3c03,0x0c60,0x1c41,
                                            0xedae,0xfd8f,0xcdec,0xddcd,0xad2a,0xbd0b,0x8d68,0x9d49,
                                            0x7e97,0x6eb6,0x5ed5,0x4ef4,0x3e13,0x2e32,0x1e51,0x0e70,
                                            0xff9f,0xefbe,0xdfdd,0xcffc,0xbf1b,0xaf3a,0x9f59,0x8f78,
                                            0x9188,0x81a9,0xb1ca,0xa1eb,0xd10c,0xc12d,0xf14e,0xe16f,
                                            0x1080,0x00a1,0x30c2,0x20e3,0x5004,0x4025,0x7046,0x6067,
                                            0x83b9,0x9398,0xa3fb,0xb3da,0xc33d,0xd31c,0xe37f,0xf35e,
                                            0x02b1,0x1290,0x22f3,0x32d2,0x4235,0x5214,0x6277,0x7256,
                                            0xb5ea,0xa5cb,0x95a8,0x8589,0xf56e,0xe54f,0xd52c,0xc50d,
                                            0x34e2,0x24c3,0x14a0,0x0481,0x7466,0x6447,0x5424,0x4405,
                                            0xa7db,0xb7fa,0x8799,0x97b8,0xe75f,0xf77e,0xc71d,0xd73c,
                                            0x26d3,0x36f2,0x0691,0x16b0,0x6657,0x7676,0x4615,0x5634,
                                            0xd94c,0xc96d,0xf90e,0xe92f,0x99c8,0x89e9,0xb98a,0xa9ab,
                                            0x5844,0x4865,0x7806,0x6827,0x18c0,0x08e1,0x3882,0x28a3,
                                            0xcb7d,0xdb5c,0xeb3f,0xfb1e,0x8bf9,0x9bd8,0xabbb,0xbb9a,
                                            0x4a75,0x5a54,0x6a37,0x7a16,0x0af1,0x1ad0,0x2ab3,0x3a92,
                                            0xfd2e,0xed0f,0xdd6c,0xcd4d,0xbdaa,0xad8b,0x9de8,0x8dc9,
                                            0x7c26,0x6c07,0x5c64,0x4c45,0x3ca2,0x2c83,0x1ce0,0x0cc1,
                                            0xef1f,0xff3e,0xcf5d,0xdf7c,0xaf9b,0xbfba,0x8fd9,0x9ff8,
                                            0x6e17,0x7e36,0x4e55,0x5e74,0x2e93,0x3eb2,0x0ed1,0x1ef0
                                           };

        protected static ushort CalcCRCProgMem()
        {
            uint CRC_Value = 0xFFFF; // seed

            for (int word = 0; word < Pk2.DevFile.PartsList[Pk2.ActivePart].ProgramMem; word += 2)
            {
                uint memWord = Pk2.DeviceBuffers.ProgramMemory[word];
                CRC_Calculate((byte)(memWord & 0xFF), ref CRC_Value);
                CRC_Calculate((byte)((memWord >> 8) & 0xFF), ref CRC_Value);
                CRC_Calculate((byte)((memWord >> 16) & 0xFF), ref CRC_Value);
                memWord = Pk2.DeviceBuffers.ProgramMemory[word + 1];
                CRC_Calculate((byte)((memWord >> 16) & 0xFF), ref CRC_Value);
                CRC_Calculate((byte)(memWord & 0xFF), ref CRC_Value);
                CRC_Calculate((byte)((memWord >> 8) & 0xFF), ref CRC_Value);
            }

            return (ushort)(CRC_Value & 0xFFFF);
        }

        protected static void CRC_Calculate(byte ByteValue, ref uint CRC_Value)
        {
            byte value;

            value = (byte)((CRC_Value >> (8)) ^ ByteValue);
            CRC_Value = CRC_LUT_Array[value] ^ (CRC_Value << 8);

        }

        protected virtual bool _PE_BlankCheck(uint lengthWords) { return false; }

        public virtual bool PE_BlankCheck(string saveText) 
        {
            if (!PE_DownloadAndConnect())
            {
                return false;
            }

            UpdateStatusWinText(saveText);

            // Check Program Memory ====================================================================================
            if (!_PE_BlankCheck((uint)Pk2.DevFile.PartsList[Pk2.ActivePart].ProgramMem))
            {
                Pk2.RunScript(KONST.PROG_EXIT, 1);
                restoreICSPSpeed();
                return false;
            }

            Pk2.RunScript(KONST.PROG_EXIT, 1);
            restoreICSPSpeed();
            return true;
        }

        public virtual bool PE_Read(string saveText) { return false; }

        public virtual bool PE_Verify(string saveText) { return false; }

        public virtual bool PE_Verify(string saveText, bool writeVerify, int lastLocation) { return false; }

        public virtual bool PE_Write(int endOfBuffer, string saveText) { return false; }

        public virtual bool PE_Write(int endOfBuffer, string saveText, bool writeVerify) { return false; }

        public virtual bool PE_Connect() { return false; }

        public virtual bool DownloadPE() { return false; }

        public bool PE_DownloadAndConnect()
        {
            // VDD must already be on!
            // reduce PE comm speed to 500kHz max
            ICSPSpeedRestore = Pk2.LastICSPSpeed;
            if (Pk2.LastICSPSpeed < 2)
                Pk2.SetProgrammingSpeed(2);

            // See if the PE already exists
            if (!PE_Connect())
            { // it doesn't, download it    
                UpdateStatusWinText("Downloading Programming Executive...");
                if (!DownloadPE())
                { // download failed
                    UpdateStatusWinText("Downloading Programming Executive...FAILED!");
                    restoreICSPSpeed();
                    return false;
                }
                if (!PE_Connect())
                { // try connecting
                    UpdateStatusWinText("Downloading Programming Executive...FAILED!");
                    restoreICSPSpeed();
                    return false;
                }
            }

            return true;
        }

        protected bool _PE_Connect( Int32 lst32ExecMemory)
        {
            Pk2.RunScript(KONST.PROG_ENTRY, 1);

            if (Pk2.DevFile.PartsList[Pk2.ActivePart].ProgMemWrPrepScript != 0)
            { // if prog mem address set script exists for this part
                Pk2.DownloadAddress3(lst32ExecMemory); // last 32 words of exec memory
                Pk2.RunScript(KONST.PROGMEM_ADDRSET, 1);
            }
            byte[] upload_buffer = new byte[KONST.UploadBufferSize];
            //Pk2.RunScriptUploadNoLen2(KONST.PROGMEM_RD, 1);
            Pk2.RunScriptUploadNoLen(KONST.PROGMEM_RD, 1);
            Array.Copy(Pk2.Usb_read_array, 1, upload_buffer, 0, KONST.USB_REPORTLENGTH);
            //Pk2.GetUpload();
            Pk2.UploadDataNoLen();
            Array.Copy(Pk2.Usb_read_array, 1, upload_buffer, KONST.USB_REPORTLENGTH, KONST.USB_REPORTLENGTH);
            // check ID word
            int memValue = (int)upload_buffer[72]; // addresss 0x800FF0
            memValue |= (int)(upload_buffer[73] << 8);
            if (memValue != PE_ID)
            {
                Pk2.RunScript(KONST.PROG_EXIT, 1);
                return false;
            }
            // check PE version
            memValue = (int)upload_buffer[75]; // addresss 0x800FF2
            memValue |= (int)(upload_buffer[76] << 8);
            if (memValue != PE_Version)
            {
                Pk2.RunScript(KONST.PROG_EXIT, 1);
                return false;
            }

            Pk2.RunScript(KONST.PROG_EXIT, 1);

            // It looks like there is a PE there.  Try talking to the PE directly:
            int commOffSet = 0;
            byte[] commandArrayp = new byte[64];
            // entering programming mode with PE (4D434850)
            commandArrayp[commOffSet++] = KONST.EXECUTE_SCRIPT;
            commandArrayp[commOffSet++] = 0; // fill in later
            commandArrayp[commOffSet++] = KONST._VPP_OFF;
            commandArrayp[commOffSet++] = KONST._MCLR_GND_ON;
            commandArrayp[commOffSet++] = KONST._VPP_PWM_ON;
            commandArrayp[commOffSet++] = KONST._BUSY_LED_ON;
            commandArrayp[commOffSet++] = KONST._SET_ICSP_PINS;
            commandArrayp[commOffSet++] = 0x00;
            commandArrayp[commOffSet++] = KONST._DELAY_LONG;
            commandArrayp[commOffSet++] = 20;
            commandArrayp[commOffSet++] = KONST._MCLR_GND_OFF;
            commandArrayp[commOffSet++] = KONST._VPP_ON;
            commandArrayp[commOffSet++] = KONST._DELAY_SHORT;
            commandArrayp[commOffSet++] = 23;
            commandArrayp[commOffSet++] = KONST._VPP_OFF;
            commandArrayp[commOffSet++] = KONST._MCLR_GND_ON;
            commandArrayp[commOffSet++] = KONST._DELAY_SHORT;
            commandArrayp[commOffSet++] = 47;
            commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL;
            commandArrayp[commOffSet++] = 0xB2;
            commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL;
            commandArrayp[commOffSet++] = 0xC2;
            commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL;
            commandArrayp[commOffSet++] = 0x12;
            commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL;
            commandArrayp[commOffSet++] = 0x0A;
            commandArrayp[commOffSet++] = KONST._MCLR_GND_OFF;
            commandArrayp[commOffSet++] = KONST._VPP_ON;
            commandArrayp[commOffSet++] = KONST._DELAY_LONG;
            commandArrayp[commOffSet++] = 6;    //32ms
            commandArrayp[1] = (byte)(commOffSet - 2);  // script length
            for (; commOffSet < 64; commOffSet++)
            {
                commandArrayp[commOffSet] = KONST.END_OF_BUFFER;
            }
            Pk2.writeUSB(commandArrayp);
            // Try sanity Check
            commOffSet = 0;
            commandArrayp[commOffSet++] = KONST.EXECUTE_SCRIPT;
            commandArrayp[commOffSet++] = 12;
            commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL; // sanity check = 0x00 01
            commandArrayp[commOffSet++] = 0x00;
            commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL;
            commandArrayp[commOffSet++] = 0x80;                     // PE talks MSB first, script routines are LSB first.
            commandArrayp[commOffSet++] = KONST._SET_ICSP_PINS;
            commandArrayp[commOffSet++] = 0x02; // PGD is input
            commandArrayp[commOffSet++] = KONST._DELAY_SHORT;
            commandArrayp[commOffSet++] = 5;                        //100+ us
            commandArrayp[commOffSet++] = KONST._READ_BYTE_BUFFER;
            commandArrayp[commOffSet++] = KONST._READ_BYTE_BUFFER;
            commandArrayp[commOffSet++] = KONST._READ_BYTE_BUFFER;
            commandArrayp[commOffSet++] = KONST._READ_BYTE_BUFFER;
            commandArrayp[commOffSet++] = KONST.UPLOAD_DATA;
            for (; commOffSet < 64; commOffSet++)
            {
                commandArrayp[commOffSet] = KONST.END_OF_BUFFER;
            }
            Pk2.writeUSB(commandArrayp);
            if (!Pk2.readUSB())
            {
                Pk2.RunScript(KONST.PROG_EXIT, 1);
                return false;
            }
            if (Pk2.Usb_read_array[1] != 4) // expect 4 bytes back : 0x10 00 00 02
            {
                Pk2.RunScript(KONST.PROG_EXIT, 1);
                return false;
            }
            if ((Pk2.Usb_read_array[2] != 0x08) || (Pk2.Usb_read_array[3] != 0x00)
                || (Pk2.Usb_read_array[4] != 0x00) || (Pk2.Usb_read_array[5] != 0x40))
            {
                Pk2.RunScript(KONST.PROG_EXIT, 1);
                return false;
            }

            // Do not exit programming mode if we successfully find a PE
            return true;
        }

        protected static void restoreICSPSpeed()
        {
            if (ICSPSpeedRestore != Pk2.LastICSPSpeed)
                Pk2.SetProgrammingSpeed(ICSPSpeedRestore);
        }

    }
}

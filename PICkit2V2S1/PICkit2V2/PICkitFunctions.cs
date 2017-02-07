using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using USB = PICkit2V2.USB;
using BT = PICkit2V2.BT;
using KONST = PICkit2V2.Constants;
using UTIL = PICkit2V2.Utilities;
using System.IO;
using System.IO.Ports;
using File = System.IO.File;
using P32 = PICkit2V2.PIC32MXFunctions;
using P32MM = PICkit2V2.PIC32MMFunctions;
using DTBL = System.Data.DataTable;
using System.Data;

namespace PICkit2V2
{
    public class PICkitFunctions
    {
        //xx timijk 2015.06.10 add PICmicro for abstraction
        public static PICmicro ActivePICmicro;

        public static String FirmwareVersion = "NA";
        public static String DeviceFileVersion = "NA";
        public static DeviceFile DevFile = new DeviceFile();
        public static DeviceData DeviceBuffers;
        public static byte[] Usb_write_array = new byte[KONST.PACKET_SIZE];
        public static byte[] Usb_read_array = new byte[KONST.PACKET_SIZE];
        public static int ActivePart = 0;
        public static uint LastDeviceID = 0;
        public static int LastDeviceRev = 0;
        public static bool LearnMode = false;
        public static byte LastICSPSpeed = 0;

        public static SerialPort spHandle = null;

        //timijk 2016.01.22 performance tuning
        private static uint writeBT_cnt = 0;
        private static double writeBT_acc_response_time1 = 0.0;
        private static double writeBT_acc_response_time2 = 0.0;
        private static double writeBT_acc_response_time2Max = 0.0;
        private static int writeBT_total_tick = 0;

        private static int writeBT_curr_latency_count = 0;
        private static int writeBT_latency_count = 0;

        private static IntPtr usbReadHandle = IntPtr.Zero;
        private static IntPtr usbWriteHandle = IntPtr.Zero;
        //timijk 2016.01.01 private static SerialPort spHandle = null;
        private static ushort lastPk2number = 0xFF;
        private static int[] familySearchTable; // index is search priority, value is family array index.
        private static bool vddOn = false;
        private static float vddLastSet = 3.3F;  // needed when a family VPP=VDD (PIC18J, PIC24, etc.)
        private static bool targetSelfPowered = false;
        private static bool fastProgramming = true;
        private static bool assertMCLR = false;
        private static bool vppFirstEnabled = false;
        private static bool lvpEnabled = false;
        private static uint scriptBufferChecksum = 0;
        private static int lastFoundPart = 0;
        private static scriptRedirect[] scriptRedirectTable = new scriptRedirect[32]; // up to 32 scripts in FW
        private struct scriptRedirect
        {
            public byte redirectToScriptLocation;
            public int deviceFileScriptNumber;
        };
        //private static int USB_BYTE_COUNT = 0;

        public static void TestingMethod()
        {
        }

        public static bool CheckComm()
        {
            Program.mCmdLogScripts.WriteLine("[CheckComm]");

            byte[] commandArray = new byte[17];
            commandArray[0] = KONST.CLR_DOWNLOAD_BUFFER;
            commandArray[1] = KONST.DOWNLOAD_DATA;
            commandArray[2] = 8;
            commandArray[3] = 0x01;
            commandArray[4] = 0x02;
            commandArray[5] = 0x03;
            commandArray[6] = 0x04;
            commandArray[7] = 0x05;
            commandArray[8] = 0x06;
            commandArray[9] = 0x07;
            commandArray[10] = 0x08;
            commandArray[11] = KONST.COPY_RAM_UPLOAD;   // DL buffer starts at 0x100
            commandArray[12] = 0x00;
            commandArray[13] = 0x01;
            commandArray[14] = KONST.UPLOAD_DATA;
            commandArray[15] = KONST.CLR_DOWNLOAD_BUFFER;
            commandArray[16] = KONST.CLR_UPLOAD_BUFFER;
            if (writeUSB(commandArray))
            {
                if (readUSB())
                {
                    if (Usb_read_array[1] == 63)
                    {
                        for (int i = 1; i < 9; i++)
                        {
                            if (Usb_read_array[1 + i] != i)
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool EnterLearnMode(byte memsize)
        {
            Program.mCmdLogScripts.WriteLine("[EnterLearnMode]");

            byte[] commandArray = new byte[5];
            commandArray[0] = KONST.ENTER_LEARN_MODE;
            commandArray[1] = 0x50;
            commandArray[2] = 0x4B;
            commandArray[3] = 0x32;
            commandArray[4] = memsize;    // PICkit 2 EEPROM size 0 = 128K, 1 = 256K
            if (writeUSB(commandArray))
            {
                LearnMode = true;
                // Set VPP voltage by family
                float vpp = DevFile.Families[GetActiveFamily()].Vpp;
                if ((vpp < 1) || (lvpEnabled && (DevFile.PartsList[ActivePart].LVPScript > 0)))
                { // When nominally zero, use VDD voltage
                    //UNLESS it's not an LVP script but a HV script (PIC24F-KA-)
                    if (lvpEnabled && (DevFile.PartsList[ActivePart].LVPScript > 0))
                    {
                        string scriptname = DevFile.Scripts[DevFile.PartsList[ActivePart].LVPScript - 1].ScriptName;
                        scriptname = scriptname.Substring(scriptname.Length - 2);
                        if (scriptname == "HV")
                        {
                            // the VPP voltage value is the 2nd script element in 100mV increments.
                            vpp = (float)DevFile.Scripts[DevFile.PartsList[ActivePart].LVPScript - 1].Script[1] / 10F;
                            SetVppVoltage(vpp, 0.7F);
                        }
                        else
                        {
                            SetVppVoltage(vddLastSet, 0.7F);
                        }
                    }
                    else
                    {
                        SetVppVoltage(vddLastSet, 0.7F);
                    }
                }
                else
                {
                    SetVppVoltage(vpp, 0.7F);
                }
                downloadPartScripts(GetActiveFamily());
                return true;
            }
            return false;
        }

        public static bool ExitLearnMode()
        {
            Program.mCmdLogScripts.WriteLine("[ExitLearnMode]");
            LearnMode = false;
            byte[] commandArray = new byte[1];
            commandArray[0] = KONST.EXIT_LEARN_MODE;
            return writeUSB(commandArray);
        }

        public static bool EnablePK2GoMode(byte memsize)
        {
            Program.mCmdLogScripts.WriteLine("[EnablePK2GoMode]");
            LearnMode = false;
            byte[] commandArray = new byte[5];
            commandArray[0] = KONST.ENABLE_PK2GO_MODE;
            commandArray[1] = 0x50;
            commandArray[2] = 0x4B;
            commandArray[3] = 0x32;
            commandArray[4] = memsize;    // PICkit 2 EEPROM size 0 = 128K, 1 = 256K

            return writeUSB(commandArray);
        }

        public static bool MetaCmd_CHECK_DEVICE_ID()
        {
            Program.mCmdLogScripts.WriteLine("[MetaCmd_CHECK_DEVICE_ID]");

            int mask = (int)(DevFile.Families[GetActiveFamily()].DeviceIDMask);
            int deviceID = (int)(DevFile.PartsList[ActivePart].DeviceID);
            if (DevFile.Families[GetActiveFamily()].ProgMemShift != 0)
            {
                mask <<= 1;
                deviceID <<= 1;
            }
            byte[] commandArray = new byte[5];
            commandArray[0] = KONST.MC_CHECK_DEVICE_ID;
            commandArray[1] = (byte)(mask & 0xFF);              // device ID mask
            commandArray[2] = (byte)((mask >> 8) & 0xFF);
            commandArray[3] = (byte)(deviceID & 0xFF);          // device ID value
            commandArray[4] = (byte)((deviceID >> 8) & 0xFF);

            return writeUSB(commandArray);
        }

        public static bool MetaCmd_READ_BANDGAP()
        {
            Program.mCmdLogScripts.WriteLine("[MetaCmd_READ_BANDGAP]");
            byte[] commandArray = new byte[1];
            commandArray[0] = KONST.MC_READ_BANDGAP;
            return writeUSB(commandArray);
        }

        public static bool MetaCmd_WRITE_CFG_BANDGAP()
        {
            Program.mCmdLogScripts.WriteLine("[MetaCmd_WRITE_CFG_BANDGAP]");
            byte[] commandArray = new byte[1];
            commandArray[0] = KONST.MC_WRITE_CFG_BANDGAP;
            return writeUSB(commandArray);
        }

        public static bool MetaCmd_READ_OSCCAL()
        {
            Program.mCmdLogScripts.WriteLine("[MetaCmd_READ_OSCCAL]");
            int OSCCALaddress = (int)(DevFile.PartsList[ActivePart].ProgramMem - 1);
            byte[] commandArray = new byte[3];
            commandArray[0] = KONST.MC_READ_OSCCAL;
            commandArray[1] = (byte)(OSCCALaddress & 0xFF);    // OSCALL address
            commandArray[2] = (byte)((OSCCALaddress >> 8) & 0xFF);

            return writeUSB(commandArray);
        }

        public static bool MetaCmd_WRITE_OSCCAL()
        {
            Program.mCmdLogScripts.WriteLine("[MetaCmd_WRITE_OSCCAL]");
            int OSCCALaddress = (int)(DevFile.PartsList[ActivePart].ProgramMem - 1);
            byte[] commandArray = new byte[3];
            commandArray[0] = KONST.MC_WRITE_OSCCAL;
            commandArray[1] = (byte)(OSCCALaddress & 0xFF);    // OSCALL address
            commandArray[2] = (byte)((OSCCALaddress >> 8) & 0xFF);

            return writeUSB(commandArray);
        }

        public static bool MetaCmd_START_CHECKSUM()
        {
            Program.mCmdLogScripts.WriteLine("[MetaCmd_START_CHECKSUM]");
            byte[] commandArray = new byte[3];
            commandArray[0] = KONST.MC_START_CHECKSUM;
            commandArray[1] = (byte)DevFile.Families[GetActiveFamily()].ProgMemShift;    //Format
            commandArray[2] = 0;

            return writeUSB(commandArray);
        }

        public static bool MetaCmd_CHANGE_CHKSM_FRMT(byte format)
        {
            Program.mCmdLogScripts.WriteLine("[MetaCmd_CHANGE_CHKSM_FRMT]");

            byte[] commandArray = new byte[3];
            commandArray[0] = KONST.MC_CHANGE_CHKSM_FRMT;
            commandArray[1] = format;    //Format
            commandArray[2] = 0;

            return writeUSB(commandArray);
        }

        public static bool MetaCmd_VERIFY_CHECKSUM(uint checksum)
        {
            Program.mCmdLogScripts.WriteLine("[MetaCmd_VERIFY_CHECKSUM]");
            checksum = ~checksum;

            byte[] commandArray = new byte[3];
            commandArray[0] = KONST.MC_VERIFY_CHECKSUM;
            commandArray[1] = (byte)(checksum & 0xFF);    // OSCALL address
            commandArray[2] = (byte)((checksum >> 8) & 0xFF);

            return writeUSB(commandArray);
        }

        public static void ResetPk2Number()
        {
            Program.mCmdLogScripts.WriteLine("[ResetPk2Number]");
            lastPk2number = 0xFF;
        }

        public static float MeasurePGDPulse()
        {   // !! ASSUMES target is powered !!
            // sets PGC as output, PGD input
            // Asserts PGC (=1) then measures pulse on PGD
            // Leaves PGC/PGD as inputs.
            //
            // Return is pulse length in ms units.
            Program.mCmdLogScripts.WriteLine("[MeasurePGDPulse]");
            byte[] commandArray = new byte[13];
            commandArray[0] = KONST.CLR_UPLOAD_BUFFER;
            commandArray[1] = KONST.EXECUTE_SCRIPT;
            commandArray[2] = 9;
            commandArray[3] = KONST._SET_ICSP_PINS;
            commandArray[4] = 0x02;
            commandArray[5] = KONST._DELAY_LONG;
            commandArray[6] = 20;                 // wait 100ms
            commandArray[7] = KONST._SET_ICSP_PINS;
            commandArray[8] = 0x06;                 // signal ready for pulse
            commandArray[9] = KONST._MEASURE_PULSE;
            commandArray[10] = KONST._SET_ICSP_PINS;
            commandArray[11] = 0x03;
            commandArray[12] = KONST.UPLOAD_DATA;   // get data
            if (writeUSB(commandArray))
            {
                if (readUSB())
                {
                    // expect 2 bytes
                    if (Usb_read_array[1] == 2)
                    {
                        float ret = (float)(Usb_read_array[2] + (Usb_read_array[3] * 0x100));
                        // ret = 0xFFFF on a timeout.
                        return (ret * .021333F);
                    }
                }
            }
            return 0F;  // failed
        }

        public static bool EnterUARTMode(uint baudValue)
        {
            Program.mCmdLogScripts.WriteLine("[EnterUARTMode]");
            byte[] commandArray = new byte[5];
            commandArray[0] = KONST.CLR_DOWNLOAD_BUFFER;
            commandArray[1] = KONST.CLR_UPLOAD_BUFFER;
            commandArray[2] = KONST.ENTER_UART_MODE;
            commandArray[3] = (byte)(baudValue & 0xFF);
            commandArray[4] = (byte)((baudValue >> 8) & 0xFF);
            return writeUSB(commandArray);
        }

        public static bool ExitUARTMode()
        {
            Program.mCmdLogScripts.WriteLine("[ExitUARTMode]");
            byte[] commandArray = new byte[3];
            commandArray[0] = KONST.EXIT_UART_MODE;
            commandArray[1] = KONST.CLR_DOWNLOAD_BUFFER;
            commandArray[2] = KONST.CLR_UPLOAD_BUFFER;
            return writeUSB(commandArray);
        }

        public static bool ValidateOSSCAL()
        {
            Program.mCmdLogScripts.WriteLine("[ValidateOSSCAL]");
            uint value = DeviceBuffers.OSCCAL;
            value &= 0xFF00;
            if ((value != 0) && (value == DevFile.PartsList[ActivePart].ConfigMasks[KONST.OSCCAL_MASK]))
            {
                return true;
            }
            return false;
        }

        public static bool isCalibrated()
        {
            Program.mCmdLogScripts.WriteLine("[isCalibrated]");
            byte[] commandArray = new byte[3];
            commandArray[0] = KONST.RD_INTERNAL_EE;
            commandArray[1] = KONST.ADC_CAL_L;
            commandArray[2] = 4;
            if (writeUSB(commandArray))
            {
                if (readUSB())
                {
                    int adcCal = Usb_read_array[1] + (Usb_read_array[2] * 0x100);
                    if ((adcCal <= 0x140) && (adcCal >= 0xC0))
                    { // ADC cal within limits.
                        if ((Usb_read_array[1] == 0x00) && (Usb_read_array[2] == 0x01)
                            && (Usb_read_array[3] == 0x00) && (Usb_read_array[4] == 0x80))
                        {// but not default cals
                            return false;
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        public static string UnitIDRead()
        { // returns a zero-length string if no ID.
            string unitID = "";

            byte[] commandArray = new byte[3];
            commandArray[0] = KONST.RD_INTERNAL_EE;
            commandArray[1] = KONST.UNIT_ID;
            commandArray[2] = 16;
            if (writeUSB(commandArray))
            {
                if (readUSB())
                {
                    if (Usb_read_array[1] == 0x23)
                    {
                        byte[] readBytes;
                        int i = 0;
                        for (; i < 15; i++)
                        {
                            if (Usb_read_array[2 + i] == 0)
                            {
                                break;
                            }
                        }
                        readBytes = new byte[i];
                        Array.Copy(Usb_read_array, 2, readBytes, 0, i);
                        char[] asciiChars = new char[Encoding.ASCII.GetCharCount(readBytes, 0, readBytes.Length)];
                        Encoding.ASCII.GetChars(readBytes, 0, readBytes.Length, asciiChars, 0);
                        string newString = new string(asciiChars);
                        unitID = newString;
                    }

                }
            }

            return unitID;
        }

        public static bool UnitIDWrite(string unitID)
        {
            int length = unitID.Length;
            if (length > 15)
            {
                length = 15;
            }
            byte[] commandArray = new byte[4 + 15];
            commandArray[0] = KONST.WR_INTERNAL_EE;
            commandArray[1] = KONST.UNIT_ID;
            commandArray[2] = 0x10;
            byte[] unicodeBytes = Encoding.Unicode.GetBytes(unitID);
            byte[] asciiBytes = Encoding.Convert(Encoding.Unicode, Encoding.ASCII, unicodeBytes);
            if (length > 0)
            {
                commandArray[3] = 0x23; // '#' first byte is always ASCII pound to indicate valid UnitID
            }
            else
            {
                commandArray[3] = 0xFF; // clear UnitID
            }

            for (int i = 0; i < 15; i++)
            {
                if (i < length)
                {
                    commandArray[4 + i] = asciiBytes[i];
                }
                else
                {
                    commandArray[4 + i] = 0;
                }
            }

            return writeUSB(commandArray);

        }

        public static bool SetVoltageCals(ushort adcCal, byte vddOffset, byte VddCal)
        {
            byte[] commandArray = new byte[5];
            commandArray[0] = KONST.SET_VOLTAGE_CALS;
            commandArray[1] = (byte)adcCal;
            commandArray[2] = (byte)(adcCal >> 8);
            commandArray[3] = vddOffset;
            commandArray[4] = VddCal;
            return writeUSB(commandArray);
        }

        public static bool HCS360_361_VppSpecial()
        {
            if (DevFile.PartsList[ActivePart].DeviceID != 0xFFFFFF36)
            { // only HCS360, 361 need this.
                return true;
            }

            byte[] commandArray = new byte[12];
            commandArray[0] = KONST.EXECUTE_SCRIPT;
            commandArray[1] = 10;
            if ((DeviceBuffers.ProgramMemory[0] & 0x1) == 0)
            { // bit 0 word 0 is 0
                commandArray[2] = KONST._SET_ICSP_PINS; // data goes low with VPP staying low, clock high
                commandArray[3] = 0x04;
                commandArray[4] = KONST._MCLR_GND_ON;
                commandArray[5] = KONST._VPP_OFF;
                commandArray[6] = KONST._DELAY_LONG;
                commandArray[7] = 0x05;
                commandArray[8] = KONST._SET_ICSP_PINS; // data set to bit 0 word 0, clock high
                commandArray[9] = 0x04;
                commandArray[10] = KONST._SET_ICSP_PINS; // clock low, data keeps value
                commandArray[11] = 0x00;
            }
            else
            { // bit 0 word 0 is 1
                commandArray[2] = KONST._SET_ICSP_PINS; // data goes low with VPP high, clock high
                commandArray[3] = 0x04;
                commandArray[4] = KONST._MCLR_GND_OFF;
                commandArray[5] = KONST._VPP_ON;
                commandArray[6] = KONST._DELAY_LONG;
                commandArray[7] = 0x05;
                commandArray[8] = KONST._SET_ICSP_PINS; // data set to bit 0 word 0, clock high
                commandArray[9] = 0x0C;
                commandArray[10] = KONST._SET_ICSP_PINS; // clock low, data keeps value
                commandArray[11] = 0x08;
            }
            return writeUSB(commandArray);

        }

        public static bool FamilyIsEEPROM()
        {
            int maxLength = DevFile.Families[GetActiveFamily()].FamilyName.Length;
            if (maxLength > 6)
            {
                maxLength = 6;
            }
            return (DevFile.Families[GetActiveFamily()].FamilyName.Substring(0, maxLength) == "EEPROM");
        }

        public static bool FamilyIsKeeloq()
        {
            return (DevFile.Families[GetActiveFamily()].FamilyName == "KEELOQ?HCS");
        }

        public static bool FamilyIsMCP()
        {
            int maxLength = DevFile.Families[GetActiveFamily()].FamilyName.Length;
            if (maxLength > 3)
            {
                maxLength = 3;
            }
            return (DevFile.Families[GetActiveFamily()].FamilyName.Substring(0, maxLength) == "MCP");
        }

        public static bool FamilyIsPIC32()
        {
            int maxLength = DevFile.Families[GetActiveFamily()].FamilyName.Length;
            if (maxLength > 5)
            {
                maxLength = 5;
            }
            return (DevFile.Families[GetActiveFamily()].FamilyName.Substring(0, maxLength) == "PIC32");
        }

        public static bool FamilyIsPIC32MX()
        {
            int maxLength = DevFile.Families[GetActiveFamily()].FamilyName.Length;
            if (maxLength > 7)
            {
                maxLength = 7;
            }
            return (DevFile.Families[GetActiveFamily()].FamilyName.Substring(0, maxLength) == "PIC32MX");
        }

        public static bool FamilyIsPIC32MM()
        {
            int maxLength = DevFile.Families[GetActiveFamily()].FamilyName.Length;
            if (maxLength > 7)
            {
                maxLength = 7;
            }
            return (DevFile.Families[GetActiveFamily()].FamilyName.Substring(0, maxLength) == "PIC32MM");
        }

        public static bool FamilyIsdsPIC30()
        {
            int maxLength = DevFile.Families[GetActiveFamily()].FamilyName.Length;
            if (maxLength > 7)
            {
                maxLength = 7;
            }
            return (DevFile.Families[GetActiveFamily()].FamilyName.Substring(0, maxLength) == "dsPIC30");
        }

        public static bool FamilyIsdsPIC30SMPS()
        {
            int maxLength = DevFile.Families[GetActiveFamily()].FamilyName.Length;
            if (maxLength > 9)
            {
                maxLength = 9;
            }
            return (DevFile.Families[GetActiveFamily()].FamilyName.Substring(0, maxLength) == "dsPIC30 S");
        }

        public static bool FamilyIsPIC18J()
        {
            int maxLength = DevFile.Families[GetActiveFamily()].FamilyName.Length;
            if (maxLength > 9)
            {
                maxLength = 9;
            }
            return (DevFile.Families[GetActiveFamily()].FamilyName.Substring(0, maxLength) == "PIC18F_J_");
        }

        public static bool FamilyIsPIC24FJ()
        {
            int maxLength = DevFile.PartsList[ActivePart].PartName.Length;
            if (maxLength > 7)
            {
                maxLength = 7;
            }
            return (DevFile.PartsList[ActivePart].PartName.Substring(0, maxLength) == "PIC24FJ");
        }

        public static bool FamilyIsPIC24H()
        {
            int maxLength = DevFile.PartsList[ActivePart].PartName.Length;
            if (maxLength > 6)
            {
                maxLength = 6;
            }
            return (DevFile.PartsList[ActivePart].PartName.Substring(0, maxLength) == "PIC24H");
        }

        public static bool FamilyIsdsPIC33F()
        {
            int maxLength = DevFile.PartsList[ActivePart].PartName.Length;
            if (maxLength > 8)
            {
                maxLength = 8;
            }
            return (DevFile.PartsList[ActivePart].PartName.Substring(0, maxLength) == "dsPIC33F");
        }

        public static bool FamilyIsdsPIC33EP()
        {
            int maxLength = DevFile.PartsList[ActivePart].PartName.Length;
            if (maxLength > 9)
            {
                maxLength = 9;
            }
            return (DevFile.PartsList[ActivePart].PartName.Substring(0, maxLength) == "dsPIC33EP");
        }

        public static void SetVPPFirstProgramEntry()
        {
            vppFirstEnabled = true;
            scriptBufferChecksum = ~scriptBufferChecksum; // force redownload of scripts
        }

        public static void ClearVppFirstProgramEntry()
        {
            vppFirstEnabled = false;
            scriptBufferChecksum = ~scriptBufferChecksum; // force redownload of scripts
        }

        public static void SetLVPProgramEntry()
        {
            lvpEnabled = true;
            scriptBufferChecksum = ~scriptBufferChecksum; // force redownload of scripts
        }

        public static void ClearLVPProgramEntry()
        {
            lvpEnabled = false;
            scriptBufferChecksum = ~scriptBufferChecksum; // force redownload of scripts
        }

        public static void RowEraseDevice()
        {
            // row erase script automatically increments PC by number of locations erased.
            // --- Erase Program Memory  ---
            int memoryRows = (int)DevFile.PartsList[ActivePart].ProgramMem / DevFile.PartsList[ActivePart].DebugRowEraseSize;
            RunScript(KONST.PROG_ENTRY, 1);
            if (DevFile.PartsList[ActivePart].ProgMemWrPrepScript != 0)
            { // if prog mem address set script exists for this part
                DownloadAddress3(0);
                RunScript(KONST.PROGMEM_WR_PREP, 1);
            }
            do
            {
                if (memoryRows >= 256)
                { // erase up to 256 rows at a time               
                    RunScript(KONST.ROW_ERASE, 0);  // 0 = 256 times
                    memoryRows -= 256;
                }
                else
                {
                    RunScript(KONST.ROW_ERASE, memoryRows);
                    memoryRows = 0;
                }

            } while (memoryRows > 0);
            RunScript(KONST.PROG_EXIT, 1);

            // --- Erase EEPROM Data ---
            // only dsPIC30 currently needs this done
            if (DevFile.PartsList[ActivePart].EERowEraseScript > 0)
            {
                int eeRows = (int)DevFile.PartsList[ActivePart].EEMem / DevFile.PartsList[ActivePart].EERowEraseWords;
                RunScript(KONST.PROG_ENTRY, 1);
                if (DevFile.PartsList[ActivePart].EERdPrepScript != 0)
                { // if ee mem address set script exists for this part
                    DownloadAddress3((int)DevFile.PartsList[ActivePart].EEAddr
                                        / DevFile.Families[GetActiveFamily()].EEMemBytesPerWord);
                    RunScript(KONST.EE_RD_PREP, 1);
                }
                do
                {
                    if (eeRows >= 256)
                    { // erase up to 256 rows at a time               
                        RunScript(KONST.EEROW_ERASE, 0);  // 0 = 256 times
                        eeRows -= 256;
                    }
                    else
                    {
                        RunScript(KONST.EEROW_ERASE, eeRows);
                        eeRows = 0;
                    }

                } while (eeRows > 0);
                RunScript(KONST.PROG_EXIT, 1);

            }

            // --- Erase Config Memory  ---
            if (DevFile.PartsList[ActivePart].ConfigMemEraseScript > 0)
            {
                RunScript(KONST.PROG_ENTRY, 1);
                if (DevFile.PartsList[ActivePart].ProgMemWrPrepScript != 0)
                { // if prog mem address set script exists for this part
                    DownloadAddress3((int)DevFile.PartsList[ActivePart].UserIDAddr);
                    RunScript(KONST.PROGMEM_WR_PREP, 1);
                }
                Program.mCmdLogScripts.WriteLine("[ExecuteScript:ConfigMemEraseScript]");
                ExecuteScript(DevFile.PartsList[ActivePart].ConfigMemEraseScript);
                RunScript(KONST.PROG_EXIT, 1);
            }
        }

        public static bool ExecuteScript(int scriptArrayIndex)
        {
            // IMPORTANT NOTE: THIS ALWAYS CLEARS THE UPLOAD BUFFER FIRST!

            int scriptLength;
            if (scriptArrayIndex == 0)
                return false;

            scriptLength = DevFile.Scripts[--scriptArrayIndex].ScriptLength;

            //int scriptLength = DevFile.Scripts[--scriptArrayIndex].ScriptLength;

            byte[] commandArray = new byte[3 + scriptLength];
            commandArray[0] = KONST.CLR_UPLOAD_BUFFER;
            commandArray[1] = KONST.EXECUTE_SCRIPT;
            commandArray[2] = (byte)scriptLength;
            for (int n = 0; n < scriptLength; n++)
            {
                commandArray[3 + n] = (byte)DevFile.Scripts[scriptArrayIndex].Script[n];
            }
            return writeUSB(commandArray);
        }


        public static bool GetVDDState()
        {
            return vddOn;
        }

        public static bool SetMCLRTemp(bool nMCLR)
        {
            byte[] releaseMCLRscript = new byte[1];
            if (nMCLR)
            {
                releaseMCLRscript[0] = KONST._MCLR_GND_ON;
            }
            else
            {
                releaseMCLRscript[0] = KONST._MCLR_GND_OFF;
            }
            return SendScript(releaseMCLRscript);
        }

        public static bool HoldMCLR(bool nMCLR)
        {
            assertMCLR = nMCLR;

            byte[] releaseMCLRscript = new byte[1];
            if (nMCLR)
            {
                releaseMCLRscript[0] = KONST._MCLR_GND_ON;
            }
            else
            {
                releaseMCLRscript[0] = KONST._MCLR_GND_OFF;
            }
            return SendScript(releaseMCLRscript);
        }

        public static void SetFastProgramming(bool fast)
        {
            fastProgramming = fast;
            // alter checksum so scripts will reload on next operation.
            scriptBufferChecksum = ~scriptBufferChecksum;
        }

        public static void ForcePICkitPowered()
        {
            targetSelfPowered = false;
        }

        public static void ForceTargetPowered()
        {
            targetSelfPowered = true;
        }

        public static void ReadConfigOutsideProgMem()
        {
            RunScript(KONST.PROG_ENTRY, 1);
            RunScript(KONST.CONFIG_RD, 1);
            UploadData();
            RunScript(KONST.PROG_EXIT, 1);
            int configWords = DevFile.PartsList[ActivePart].ConfigWords;
            int bufferIndex = 2;                    // report starts on index 1, which is #bytes uploaded.
            for (int word = 0; word < configWords; word++)
            {
                uint config = (uint)Usb_read_array[bufferIndex++];
                config |= (uint)Usb_read_array[bufferIndex++] << 8;
                if (DevFile.Families[GetActiveFamily()].ProgMemShift > 0)
                {
                    config = (config >> 1) & DevFile.Families[GetActiveFamily()].BlankValue;
                }
                DeviceBuffers.ConfigWords[word] = config;
            }
        }

        public static void ReadBandGap()
        {
            RunScript(KONST.PROG_ENTRY, 1);
            RunScript(KONST.CONFIG_RD, 1);
            UploadData();
            RunScript(KONST.PROG_EXIT, 1);
            int configWords = DevFile.PartsList[ActivePart].ConfigWords;
            uint config = (uint)Usb_read_array[2];
            config |= (uint)Usb_read_array[3] << 8;
            if (DevFile.Families[GetActiveFamily()].ProgMemShift > 0)
            {
                config = (config >> 1) & DevFile.Families[GetActiveFamily()].BlankValue;
            }
            DeviceBuffers.BandGap = config & DevFile.PartsList[ActivePart].BandGapMask;
        }

        public static uint WriteConfigOutsideProgMem(bool codeProtect, bool dataProtect)
        {
            int configWords = DevFile.PartsList[ActivePart].ConfigWords;
            uint checksumPk2Go = 0;
            byte[] configBuffer = new byte[configWords * 2];

            if (DevFile.PartsList[ActivePart].BandGapMask > 0)
            {
                DeviceBuffers.ConfigWords[0] &= ~DevFile.PartsList[ActivePart].BandGapMask;
                if (!LearnMode)
                    DeviceBuffers.ConfigWords[0] |= DeviceBuffers.BandGap;
            }
            if (FamilyIsMCP())
            {
                DeviceBuffers.ConfigWords[0] |= 0x3FF8;
            }

            RunScript(KONST.PROG_ENTRY, 1);

            if (DevFile.PartsList[ActivePart].ConfigWrPrepScript > 0)
            {
                //timijk 2015.06.08
                if (FamilyIsdsPIC33EP())
                { DownloadAddress3((int)(DevFile.PartsList[ActivePart].ConfigAddr / 2)); }
                else DownloadAddress3(0);
                RunScript(KONST.CONFIG_WR_PREP, 1);
            }

            for (int i = 0, j = 0; i < configWords; i++)
            {
                uint configWord = DeviceBuffers.ConfigWords[i] & DevFile.PartsList[ActivePart].ConfigMasks[i];
                if (i == DevFile.PartsList[ActivePart].CPConfig - 1)
                {
                    if (codeProtect)
                    {
                        configWord &= (uint)~DevFile.PartsList[ActivePart].CPMask;
                    }
                    if (dataProtect)
                    {
                        configWord &= (uint)~DevFile.PartsList[ActivePart].DPMask;
                    }
                }
                if (DevFile.Families[GetActiveFamily()].ProgMemShift > 0)
                { // baseline & midrange
                    configWord |= (~(uint)DevFile.PartsList[ActivePart].ConfigMasks[i] & ~DevFile.PartsList[ActivePart].BandGapMask);
                    if (!FamilyIsMCP())
                        configWord &= DevFile.Families[GetActiveFamily()].BlankValue;
                    configWord = configWord << 1;
                }
                configBuffer[j++] = (byte)(configWord & 0xFF);
                configBuffer[j++] = (byte)((configWord >> 8) & 0xFF);
                checksumPk2Go += (byte)(configWord & 0xFF);
                checksumPk2Go += (byte)((configWord >> 8) & 0xFF);
            }
            DataClrAndDownload(configBuffer, 0);

            if (LearnMode && (DevFile.PartsList[ActivePart].BandGapMask > 0))
                MetaCmd_WRITE_CFG_BANDGAP();
            else
                RunScript(KONST.CONFIG_WR, 1);
            RunScript(KONST.PROG_EXIT, 1);
            return checksumPk2Go;
        }

        public static bool ReadOSSCAL()
        {
            if (RunScript(KONST.PROG_ENTRY, 1))
            {
                if (DownloadAddress3((int)(DevFile.PartsList[ActivePart].ProgramMem - 1)))
                {
                    if (RunScript(KONST.OSSCAL_RD, 1))
                    {
                        if (UploadData())
                        {
                            if (RunScript(KONST.PROG_EXIT, 1))
                            {
                                DeviceBuffers.OSCCAL = (uint)(Usb_read_array[2] + (Usb_read_array[3] * 256));
                                if (DevFile.Families[GetActiveFamily()].ProgMemShift > 0)
                                {
                                    DeviceBuffers.OSCCAL >>= 1;
                                }
                                DeviceBuffers.OSCCAL &= DevFile.Families[GetActiveFamily()].BlankValue;
                                //DeviceBuffers.OSCCAL = 0xc00;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static bool WriteOSSCAL()
        {
            if (RunScript(KONST.PROG_ENTRY, 1))
            {
                uint calWord = DeviceBuffers.OSCCAL;
                uint calAddress = DevFile.PartsList[ActivePart].ProgramMem - 1;
                if (DevFile.Families[GetActiveFamily()].ProgMemShift > 0)
                {
                    calWord <<= 1;
                }
                byte[] addressData = new byte[5];
                addressData[0] = (byte)(calAddress & 0xFF);
                addressData[1] = (byte)((calAddress >> 8) & 0xFF);
                addressData[2] = (byte)((calAddress >> 16) & 0xFF);
                addressData[3] = (byte)(calWord & 0xFF);
                addressData[4] = (byte)((calWord >> 8) & 0xFF);
                DataClrAndDownload(addressData, 0);
                if (RunScript(KONST.OSSCAL_WR, 1))
                {
                    if (RunScript(KONST.PROG_EXIT, 1))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static KONST.PICkit2PWR CheckTargetPower(ref float vdd, ref float vpp)
        {
            if (vddOn)  // if VDD on, can't check for self-powered target
            {
                return KONST.PICkit2PWR.vdd_on;
            }

            if (ReadPICkitVoltages(ref vdd, ref vpp))
            {
                if (vdd > KONST.VddThresholdForSelfPoweredTarget)
                {
                    targetSelfPowered = true;
                    SetVDDVoltage(vdd, 0.85F);                     // set VDD to target level
                    return KONST.PICkit2PWR.selfpowered;
                }
                targetSelfPowered = false;
                return KONST.PICkit2PWR.unpowered;
            }
            targetSelfPowered = false;
            return KONST.PICkit2PWR.no_response;
        }

        public static int GetActiveFamily()
        {
            return DevFile.PartsList[ActivePart].Family;
        }

        public static void SetActiveFamily(int family)
        {
            ActivePart = 0;
            lastFoundPart = 0;
            DevFile.PartsList[ActivePart].Family = (ushort)family;
            // Set up the buffers for unsupported part (else they remain last selected part)
            ResetBuffers();
        }

        public static bool SetVDDVoltage(float voltage, float threshold)
        {
            if (voltage < 2.5F)
            {
                voltage = 2.5F;  // minimum, as when forcing VDD Target can get set very low (last reading)
                // and too low prevents VPP pump from working.
            }

            vddLastSet = voltage;

            ushort ccpValue = CalculateVddCPP(voltage);
            byte vFault = (byte)(((threshold * voltage) / 5F) * 255F);
            if (vFault > 210)
            {
                vFault = 210; // ~4.12v maximum.  Because of diode droop, limit threshold on high side.
            }

            byte[] commandArray = new byte[4];
            commandArray[0] = KONST.SETVDD;
            commandArray[1] = (byte)(ccpValue & 0xFF);
            commandArray[2] = (byte)(ccpValue / 256);
            commandArray[3] = vFault;
            return writeUSB(commandArray);
        }

        public static ushort CalculateVddCPP(float voltage)
        {
            ushort ccpValue = (ushort)(voltage * 32F + 10.5F);
            ccpValue <<= 6;
            return ccpValue;
        }

        public static bool VddOn()
        {
            byte[] commandArray = new byte[4];
            commandArray[0] = KONST.EXECUTE_SCRIPT;
            commandArray[1] = 2;
            commandArray[2] = KONST._VDD_GND_OFF;
            if (targetSelfPowered)
            {   // don't turn on VDD if self-powered!
                commandArray[3] = KONST._VDD_OFF;
            }
            else
            {
                commandArray[3] = KONST._VDD_ON;
            }
            bool result = writeUSB(commandArray);
            if (result)
            {
                vddOn = true;
                return true;
            }
            return result;
        }

        public static bool VddOff()
        {
            byte[] commandArray = new byte[4];
            commandArray[0] = KONST.EXECUTE_SCRIPT;
            commandArray[1] = 2;
            commandArray[2] = KONST._VDD_OFF;
            if (targetSelfPowered)
            { // don't ground VDD if self-powered target
                commandArray[3] = KONST._VDD_GND_OFF;
            }
            else
            {
                commandArray[3] = KONST._VDD_GND_ON;
            }
            bool result = writeUSB(commandArray);
            if (result)
            {
                vddOn = false;
                return true;
            }
            return result;
        }

        public static bool SetProgrammingSpeed(byte speed)
        {
            LastICSPSpeed = speed;

            byte[] commandArray = new byte[4];
            commandArray[0] = KONST.EXECUTE_SCRIPT;
            commandArray[1] = 2;
            commandArray[2] = KONST._SET_ICSP_SPEED;
            commandArray[3] = speed;
            return writeUSB(commandArray);
        }

        public static bool ResetPICkit2()
        {
            byte[] commandArray = new byte[1];
            commandArray[0] = KONST.RESET;
            return writeUSB(commandArray);
        }

        public static bool EnterBootloader()
        {
            byte[] commandArray = new byte[1];
            commandArray[0] = KONST.ENTER_BOOTLOADER;
            return writeUSB(commandArray);
        }

        public static bool VerifyBootloaderMode()
        {
            byte[] commandArray = new byte[1];
            commandArray[0] = KONST.FIRMWARE_VERSION;
            if (writeUSB(commandArray))
            {
                if (readUSB())
                {
                    if (Usb_read_array[1] == 118) // ASCII 'B'
                    {
                        return true;
                    }
                    return false;
                }
                return false;
            }
            return false;
        }

        public static bool BL_EraseFlash()
        {
            // timijk 2015.04.07
            //row ease operation: 64bytes (0x40)
            //PIC18F2550: 2000~7FFF, 6000 bytes, 3000*2 bytes, 0xC0*0x40*2
            //PIC18F2455: 2000~5FFF, 4000 bytes, 2000*2 bytes, 0x80*0x40*2

            //Console.WriteLine("BL_EraseFlash(1)\n");
            byte[] commandArray = new byte[5];
            commandArray[0] = KONST.ERASEFWFLASH;
            commandArray[1] = 0xC0; //# 32-word blocks to erase
            if (USB.UnitID == "PIC18F2455") commandArray[1] = 0x80;   // timijk 2015.04.07
            commandArray[2] = 0x00;
            commandArray[3] = 0x20;
            commandArray[4] = 0x00;
            if (writeUSB(commandArray, false))
            {
                //timijk 2016.01.13 timing issues
                //Console.WriteLine("BL_EraseFlash(2)\n");
                commandArray[3] = 0x50;
                if (USB.UnitID == "PIC18F2455") commandArray[3] = 0x40;  // timijk 2015.04.07
                if (writeUSB(commandArray, false))
                {
                    //Thread.Sleep(2000); //Console.WriteLine("BL_EraseFlash(OK)\n");
                    return true;
                }
            }
            return false;
        }

        public static bool BL_WriteFlash(byte[] payload)
        {
            byte[] commandArray = new byte[2 + 35];
            commandArray[0] = KONST.WRITEFWFLASH;
            commandArray[1] = 32;
            for (int i = 0; i < 35; i++)
            {
                commandArray[2 + i] = payload[i];
            }
            return writeUSB(commandArray, false);

        }

        public static bool BL_WriteFWLoadedKey()
        {
            byte[] flashWriteData = new byte[3 + 32];  // 3 address bytes plus 32 data bytes.

            flashWriteData[0] = 0xE0;
            flashWriteData[1] = 0x7F;   // for PIC18F2550  // timijk 2015.04.07
            if (USB.UnitID == "PIC18F2455") flashWriteData[1] = 0x5F;  //for PIC18F2455
            flashWriteData[2] = 0x00;   // Address = 0x007FE0
            for (int i = 3; i < flashWriteData.Length; i++)
            {
                flashWriteData[i] = 0xFF;
            }
            flashWriteData[flashWriteData.Length - 2] = 0x55;
            flashWriteData[flashWriteData.Length - 1] = 0x55;
            return BL_WriteFlash(flashWriteData);
        }

        public static bool BL_ReadFlash16(int address)
        {
            byte[] commandArray = new byte[5];
            commandArray[0] = KONST.READFWFLASH;
            commandArray[1] = 16;
            commandArray[2] = (byte)(address & 0xFF);
            commandArray[3] = (byte)((address >> 8) & 0xFF);
            commandArray[4] = 0x00;
            if (writeUSB(commandArray, false))
            {
                return readUSB();
            }
            return false;

        }

        public static bool BL_Reset()
        {
            byte[] commandArray = new byte[1];
            commandArray[0] = KONST.RESETFWDEVICE;

            return writeUSB(commandArray, false);

        }

        public static bool ButtonPressed()
        {
            ushort status = readPkStatus();
            if ((status & 0x0040) == 0x0040)
            {
                return true;
            }
            return false;
        }

        public static bool BusErrorCheck()
        {
            ushort status = readPkStatus();
            if ((status & 0x0400) == 0x0400)
            {
                return true; //error
            }

            byte[] commandArray = new byte[3];
            commandArray[0] = KONST.EXECUTE_SCRIPT;
            commandArray[1] = 1;
            commandArray[2] = KONST._BUSY_LED_ON;
            writeUSB(commandArray);

            return false; // no error
        }

        public static KONST.PICkit2PWR PowerStatus()
        {
            ushort status = readPkStatus();
            if (status == 0xFFFF)
            {
                return KONST.PICkit2PWR.no_response;
            }
            if ((status & 0x0030) == 0x0030)
            {
                vddOn = false;
                return KONST.PICkit2PWR.vddvpperrors;
            }
            if ((status & 0x0020) == 0x0020)
            {
                vddOn = false;
                return KONST.PICkit2PWR.vpperror;
            }
            if ((status & 0x0010) == 0x0010)
            {
                vddOn = false;
                return KONST.PICkit2PWR.vdderror;
            }
            if ((status & 0x0002) == 0x0002)
            {
                vddOn = true;
                return KONST.PICkit2PWR.vdd_on;
            }
            vddOn = false;
            return KONST.PICkit2PWR.vdd_off;

        }

        public static void DisconnectPICkit2Unit()
        {
            if (spHandle != null) writeBTflushACK();

            if (usbWriteHandle != IntPtr.Zero)
                USB.CloseHandle(usbWriteHandle);
            if (usbReadHandle != IntPtr.Zero)
                USB.CloseHandle(usbReadHandle);
            usbReadHandle = IntPtr.Zero;
            usbWriteHandle = IntPtr.Zero;
            spHandle = null;
        }
        public static string GetSerialUnitID()
        {
            return USB.UnitID;
        }

        public static KONST.PICkit2USB DetectPICkit2Device(ushort pk2ID, bool pk2BT, bool readFW)
        {
            IntPtr usbRdTemp = IntPtr.Zero;
            IntPtr usbWrTemp = IntPtr.Zero;
            SerialPort spTemp = null;
            bool result = false;

            DisconnectPICkit2Unit();

            if (pk2BT)
            {
                result = BT.Find_This_Device(KONST.MChipVendorID, KONST.Pk2DeviceID,
                                     pk2ID, ref spTemp);
            }
            else
            {
                result = USB.Find_This_Device(KONST.MChipVendorID, KONST.Pk2DeviceID,
                                     pk2ID, ref usbRdTemp, ref usbWrTemp);
            }

            // If we use this check and keep the old handles, we'll read whatever packets
            // were read by somebody else looking for pk2 units, messing up communications.
            //if (pk2ID != lastPk2number)
            //{ // new unit selected
            lastPk2number = pk2ID;
            // update handles
            usbReadHandle = usbRdTemp;
            usbWriteHandle = usbWrTemp;
            spHandle = spTemp;
            //}

            if (result && !readFW)
                return KONST.PICkit2USB.found;

            if (result)
            {

                //Read firmware version - this will exit PK2Go mode if needed
                byte[] commandArray = new byte[1];
                commandArray[0] = KONST.FIRMWARE_VERSION;
                result = writeUSB(commandArray);
                if (result)
                {
                    // read response
                    if (readUSB())
                    {
                        // create a version string
                        FirmwareVersion = string.Format("{0:D1}.{1:D2}.{2:D2}", Usb_read_array[1],
                                    Usb_read_array[2], Usb_read_array[3]);
                        // check for minimum supported version
                        if (Usb_read_array[1] == KONST.FWVerMajorReq)
                        {
                            if (((Usb_read_array[2] == KONST.FWVerMinorReq)
                                && (Usb_read_array[3] >= KONST.FWVerDotReq))
                                || (Usb_read_array[2] > KONST.FWVerMinorReq))
                            {
                                return KONST.PICkit2USB.found;
                            }
                        }
                        if (Usb_read_array[1] == 'v')
                        {
                            FirmwareVersion = string.Format("BL {0:D1}.{1:D1}", Usb_read_array[7], Usb_read_array[8]);
                            return KONST.PICkit2USB.bootloader;

                        }
                        return KONST.PICkit2USB.firmwareInvalid;
                    }
                    return KONST.PICkit2USB.readError;
                }
                return KONST.PICkit2USB.writeError;
            }
            return KONST.PICkit2USB.notFound;

        }

        public static bool ReadDeviceFileXML(string DeviceFileName)
        {
            string DeviceFileNameXML = DeviceFileName.Replace(".dat", ".xml");

            if (!File.Exists(DeviceFileNameXML)) return false;

            try
            {
                System.Data.DataSet dsDeviceFile = new System.Data.DataSet("DeviceFile");

                dsDeviceFile.ReadXml(DeviceFileNameXML, System.Data.XmlReadMode.ReadSchema);

                DTBL tblInfo = dsDeviceFile.Tables["Info"];
                DTBL tblFamilies = dsDeviceFile.Tables["Families"];
                DTBL tblPartsList = dsDeviceFile.Tables["PartsList"];
                DTBL tblScripts = dsDeviceFile.Tables["Scripts"];


                //
                DevFile.Info.VersionMajor = (Int32)tblInfo.Rows[0]["VersionMajor"];
                DevFile.Info.VersionMinor = (Int32)tblInfo.Rows[0]["VersionMinor"];
                DevFile.Info.VersionDot = (Int32)tblInfo.Rows[0]["VersionDot"];
                DevFile.Info.VersionNotes = (String)tblInfo.Rows[0]["VersionNotes"];
                DevFile.Info.NumberFamilies = (Int32)tblInfo.Rows[0]["NumberFamilies"];
                DevFile.Info.NumberParts = (Int32)tblInfo.Rows[0]["NumberParts"];
                DevFile.Info.NumberScripts = (Int32)tblInfo.Rows[0]["NumberScripts"];
                DevFile.Info.Compatibility = (Byte)tblInfo.Rows[0]["Compatibility"];
                DevFile.Info.UNUSED1A = (Byte)tblInfo.Rows[0]["UNUSED1A"];
                DevFile.Info.UNUSED1B = (UInt16)tblInfo.Rows[0]["UNUSED1B"];
                DevFile.Info.UNUSED2 = (UInt32)tblInfo.Rows[0]["UNUSED2"];

                // create a version string
                DeviceFileVersion = string.Format("{0:D1}.{1:D2}.{2:D2}", DevFile.Info.VersionMajor,
                                 DevFile.Info.VersionMinor, DevFile.Info.VersionDot);
                //
                // Declare arrays
                //
                DevFile.Info.NumberFamilies = tblFamilies.Rows.Count;
                DevFile.Info.NumberParts = tblPartsList.Rows.Count;
                DevFile.Info.NumberScripts = tblScripts.Rows.Count;

                DevFile.Families = new DeviceFile.DeviceFamilyParams[DevFile.Info.NumberFamilies];
                DevFile.PartsList = new DeviceFile.DevicePartParams[DevFile.Info.NumberParts];
                DevFile.Scripts = new DeviceFile.DeviceScripts[DevFile.Info.NumberScripts];

                //
                // now read all families if they are there
                //
                int l_x = 0;
                foreach (DataRow row in tblFamilies.Rows)
                {
                    DevFile.Families[l_x].FamilyID = (UInt16)row["FamilyID"];
                    DevFile.Families[l_x].FamilyType = (UInt16)row["FamilyType"];
                    DevFile.Families[l_x].SearchPriority = (UInt16)row["SearchPriority"];
                    DevFile.Families[l_x].FamilyName = (String)row["FamilyName"];
                    DevFile.Families[l_x].ProgEntryScript = (UInt16)row["ProgEntryScript"];
                    DevFile.Families[l_x].ProgExitScript = (UInt16)row["ProgExitScript"];
                    DevFile.Families[l_x].ReadDevIDScript = (UInt16)row["ReadDevIDScript"];
                    DevFile.Families[l_x].DeviceIDMask = (UInt32)row["DeviceIDMask"];
                    DevFile.Families[l_x].BlankValue = (UInt32)row["BlankValue"];
                    DevFile.Families[l_x].BytesPerLocation = (Byte)row["BytesPerLocation"];
                    DevFile.Families[l_x].AddressIncrement = (Byte)row["AddressIncrement"];
                    DevFile.Families[l_x].PartDetect = (Boolean)row["PartDetect"];
                    DevFile.Families[l_x].ProgEntryVPPScript = (UInt16)row["ProgEntryVPPScript"];
                    DevFile.Families[l_x].UNUSED1 = (UInt16)row["UNUSED1"];
                    DevFile.Families[l_x].EEMemBytesPerWord = (Byte)row["EEMemBytesPerWord"];
                    DevFile.Families[l_x].EEMemAddressIncrement = (Byte)row["EEMemAddressIncrement"];
                    DevFile.Families[l_x].UserIDHexBytes = (Byte)row["UserIDHexBytes"];
                    DevFile.Families[l_x].UserIDBytes = (Byte)row["UserIDBytes"];
                    DevFile.Families[l_x].ProgMemHexBytes = (Byte)row["ProgMemHexBytes"];
                    DevFile.Families[l_x].EEMemHexBytes = (Byte)row["EEMemHexBytes"];
                    DevFile.Families[l_x].ProgMemShift = (Byte)row["ProgMemShift"];
                    DevFile.Families[l_x].TestMemoryStart = (UInt32)row["TestMemoryStart"];
                    DevFile.Families[l_x].TestMemoryLength = (UInt16)row["TestMemoryLength"];
                    DevFile.Families[l_x].Vpp = (Single)row["Vpp"];

                    l_x++;
                }

                // Create family search table based on priority
                familySearchTable = new int[DevFile.Info.NumberFamilies];
                for (int familyIdx = 0; familyIdx < DevFile.Info.NumberFamilies; familyIdx++)
                {
                    familySearchTable[DevFile.Families[familyIdx].SearchPriority] = familyIdx;
                }

                //
                // now read all scripts if they are there
                //                    
                l_x = 0;
                foreach (DataRow row in tblScripts.Rows)
                {
                    DevFile.Scripts[l_x].ScriptNumber = (UInt16)row["ScriptNumber"];
                    DevFile.Scripts[l_x].ScriptName = (String)row["ScriptName"];
                    DevFile.Scripts[l_x].ScriptVersion = (UInt16)row["ScriptVersion"];
                    DevFile.Scripts[l_x].UNUSED1 = (UInt32)row["UNUSED1"];
                    DevFile.Scripts[l_x].ScriptLength = (UInt16)row["ScriptLength"];
                    DevFile.Scripts[l_x].Script = (ushort[])row["Script"];
                    DevFile.Scripts[l_x].Comment = (String)row["Comment"];
                    l_x++;
                }

                //addPIC32Parts(tblPartsList);

                //dsDeviceFile.WriteXml("c:\\temp\\dsDeviceFilePIC32.xml", System.Data.XmlWriteMode.WriteSchema);
                //
                // now read all parts if they are there
                //
                l_x = 0;
                foreach (DataRow row in tblPartsList.Rows)
                {
                    DevFile.PartsList[l_x].PartName = (String)row["PartName"];
                    DevFile.PartsList[l_x].Family = (UInt16)row["Family"];
                    DevFile.PartsList[l_x].DeviceID = (UInt32)row["DeviceID"] & DevFile.Families[DevFile.PartsList[l_x].Family].DeviceIDMask;
                    DevFile.PartsList[l_x].ProgramMem = (UInt32)row["ProgramMem"];
                    DevFile.PartsList[l_x].EEMem = (UInt16)row["EEMem"];
                    DevFile.PartsList[l_x].EEAddr = (UInt32)row["EEAddr"];
                    DevFile.PartsList[l_x].ConfigWords = (Byte)row["ConfigWords"];
                    DevFile.PartsList[l_x].ConfigAddr = (UInt32)row["ConfigAddr"];
                    DevFile.PartsList[l_x].UserIDWords = (Byte)row["UserIDWords"];
                    DevFile.PartsList[l_x].UserIDAddr = (UInt32)row["UserIDAddr"];
                    DevFile.PartsList[l_x].BandGapMask = (UInt32)row["BandGapMask"];
                    // Init config arrays
                    DevFile.PartsList[l_x].ConfigMasks = (ushort[])row["ConfigMasks"];
                    DevFile.PartsList[l_x].ConfigBlank = (ushort[])row["ConfigBlank"];
                    DevFile.PartsList[l_x].CPMask = (UInt16)row["CPMask"];
                    DevFile.PartsList[l_x].CPConfig = (Byte)row["CPConfig"];
                    DevFile.PartsList[l_x].OSSCALSave = (Boolean)row["OSSCALSave"];
                    DevFile.PartsList[l_x].IgnoreAddress = (UInt32)row["IgnoreAddress"];
                    DevFile.PartsList[l_x].VddMin = (Single)row["VddMin"];
                    DevFile.PartsList[l_x].VddMax = (Single)row["VddMax"];
                    DevFile.PartsList[l_x].VddErase = (Single)row["VddErase"];
                    DevFile.PartsList[l_x].CalibrationWords = (Byte)row["CalibrationWords"];
                    DevFile.PartsList[l_x].ChipEraseScript = (UInt16)row["ChipEraseScript"];
                    DevFile.PartsList[l_x].ProgMemAddrSetScript = (UInt16)row["ProgMemAddrSetScript"];
                    DevFile.PartsList[l_x].ProgMemAddrBytes = (Byte)row["ProgMemAddrBytes"];
                    DevFile.PartsList[l_x].ProgMemRdScript = (UInt16)row["ProgMemRdScript"];
                    DevFile.PartsList[l_x].ProgMemRdWords = (UInt16)row["ProgMemRdWords"];
                    DevFile.PartsList[l_x].EERdPrepScript = (UInt16)row["EERdPrepScript"];
                    DevFile.PartsList[l_x].EERdScript = (UInt16)row["EERdScript"];
                    DevFile.PartsList[l_x].EERdLocations = (UInt16)row["EERdLocations"];
                    DevFile.PartsList[l_x].UserIDRdPrepScript = (UInt16)row["UserIDRdPrepScript"];
                    DevFile.PartsList[l_x].UserIDRdScript = (UInt16)row["UserIDRdScript"];
                    DevFile.PartsList[l_x].ConfigRdPrepScript = (UInt16)row["ConfigRdPrepScript"];
                    DevFile.PartsList[l_x].ConfigRdScript = (UInt16)row["ConfigRdScript"];
                    DevFile.PartsList[l_x].ProgMemWrPrepScript = (UInt16)row["ProgMemWrPrepScript"];
                    DevFile.PartsList[l_x].ProgMemWrScript = (UInt16)row["ProgMemWrScript"];
                    DevFile.PartsList[l_x].ProgMemWrWords = (UInt16)row["ProgMemWrWords"];
                    DevFile.PartsList[l_x].ProgMemPanelBufs = (Byte)row["ProgMemPanelBufs"];
                    DevFile.PartsList[l_x].ProgMemPanelOffset = (UInt32)row["ProgMemPanelOffset"];
                    DevFile.PartsList[l_x].EEWrPrepScript = (UInt16)row["EEWrPrepScript"];
                    DevFile.PartsList[l_x].EEWrScript = (UInt16)row["EEWrScript"];
                    DevFile.PartsList[l_x].EEWrLocations = (UInt16)row["EEWrLocations"];
                    DevFile.PartsList[l_x].UserIDWrPrepScript = (UInt16)row["UserIDWrPrepScript"];
                    DevFile.PartsList[l_x].UserIDWrScript = (UInt16)row["UserIDWrScript"];
                    DevFile.PartsList[l_x].ConfigWrPrepScript = (UInt16)row["ConfigWrPrepScript"];
                    DevFile.PartsList[l_x].ConfigWrScript = (UInt16)row["ConfigWrScript"];
                    DevFile.PartsList[l_x].OSCCALRdScript = (UInt16)row["OSCCALRdScript"];
                    DevFile.PartsList[l_x].OSCCALWrScript = (UInt16)row["OSCCALWrScript"];
                    DevFile.PartsList[l_x].DPMask = (UInt16)row["DPMask"];
                    DevFile.PartsList[l_x].WriteCfgOnErase = (Boolean)row["WriteCfgOnErase"];
                    DevFile.PartsList[l_x].BlankCheckSkipUsrIDs = (Boolean)row["BlankCheckSkipUsrIDs"];
                    DevFile.PartsList[l_x].IgnoreBytes = (UInt16)row["IgnoreBytes"];
                    DevFile.PartsList[l_x].ChipErasePrepScript = (UInt16)row["ChipErasePrepScript"];
                    DevFile.PartsList[l_x].BootFlash = (UInt32)row["BootFlash"];
                    //DevFile.PartsList[l_x].UNUSED4 = (UInt32)row[""];
                    DevFile.PartsList[l_x].Config9Mask = (UInt16)row["Config9Mask"];
                    //DevFile.PartsList[l_x].ConfigMasks[8] = DevFile.PartsList[l_x].Config9Mask;
                    DevFile.PartsList[l_x].Config9Blank = (UInt16)row["Config9Blank"];
                    //DevFile.PartsList[l_x].ConfigBlank[8] = DevFile.PartsList[l_x].Config9Blank;
                    DevFile.PartsList[l_x].ProgMemEraseScript = (UInt16)row["ProgMemEraseScript"];
                    DevFile.PartsList[l_x].EEMemEraseScript = (UInt16)row["EEMemEraseScript"];
                    DevFile.PartsList[l_x].ConfigMemEraseScript = (UInt16)row["ConfigMemEraseScript"];
                    DevFile.PartsList[l_x].reserved1EraseScript = (UInt16)row["reserved1EraseScript"];
                    DevFile.PartsList[l_x].reserved2EraseScript = (UInt16)row["reserved2EraseScript"];
                    DevFile.PartsList[l_x].TestMemoryRdScript = (UInt16)row["TestMemoryRdScript"];
                    DevFile.PartsList[l_x].TestMemoryRdWords = (UInt16)row["TestMemoryRdWords"];
                    DevFile.PartsList[l_x].EERowEraseScript = (UInt16)row["EERowEraseScript"];
                    DevFile.PartsList[l_x].EERowEraseWords = (UInt16)row["EERowEraseWords"];
                    DevFile.PartsList[l_x].ExportToMPLAB = (Boolean)row["ExportToMPLAB"];
                    DevFile.PartsList[l_x].DebugHaltScript = (UInt16)row["DebugHaltScript"];
                    DevFile.PartsList[l_x].DebugRunScript = (UInt16)row["DebugRunScript"];
                    DevFile.PartsList[l_x].DebugStatusScript = (UInt16)row["DebugStatusScript"];
                    DevFile.PartsList[l_x].DebugReadExecVerScript = (UInt16)row["DebugReadExecVerScript"];
                    DevFile.PartsList[l_x].DebugSingleStepScript = (UInt16)row["DebugSingleStepScript"];
                    DevFile.PartsList[l_x].DebugBulkWrDataScript = (UInt16)row["DebugBulkWrDataScript"];
                    DevFile.PartsList[l_x].DebugBulkRdDataScript = (UInt16)row["DebugBulkRdDataScript"];
                    DevFile.PartsList[l_x].DebugWriteVectorScript = (UInt16)row["DebugWriteVectorScript"];
                    DevFile.PartsList[l_x].DebugReadVectorScript = (UInt16)row["DebugReadVectorScript"];
                    DevFile.PartsList[l_x].DebugRowEraseScript = (UInt16)row["DebugRowEraseScript"];
                    DevFile.PartsList[l_x].DebugRowEraseSize = (UInt16)row["DebugRowEraseSize"];
                    DevFile.PartsList[l_x].DebugReserved5Script = (UInt16)row["DebugReserved5Script"];
                    DevFile.PartsList[l_x].DebugReserved6Script = (UInt16)row["DebugReserved6Script"];
                    DevFile.PartsList[l_x].DebugReserved7Script = (UInt16)row["DebugReserved7Script"];
                    DevFile.PartsList[l_x].DebugReserved8Script = (UInt16)row["DebugReserved8Script"];
                    DevFile.PartsList[l_x].LVPScript = (UInt16)row["LVPScript"];

                    l_x++;
                }

            }
            catch
            {
                return false;
            }

            return true;

        }

        public static bool ReadDeviceFileEXport(string DeviceFileName)
        {
            string DeviceFileNameXML = DeviceFileName.Replace(".dat", ".xml");

            bool fileExists = File.Exists(DeviceFileName);
            DTBL tblInfo = new DTBL("Info");
            DTBL tblFamilies = new DTBL("Families");
            DTBL tblPartsList = new DTBL("PartsList");
            DTBL tblScripts = new DTBL("Scripts");

            System.Data.DataSet dsDeviceFile = new System.Data.DataSet("DeviceFile");

            dsDeviceFile.Tables.Add(tblInfo);
            dsDeviceFile.Tables.Add(tblFamilies);
            dsDeviceFile.Tables.Add(tblPartsList);
            dsDeviceFile.Tables.Add(tblScripts);

            setupDTBLs(tblInfo, tblFamilies, tblPartsList, tblScripts);

            if (fileExists)
            {
                try
                {
                    //FileStream fsDevFile = File.Open(DeviceFileName, FileMode.Open);
                    FileStream fsDevFile = File.OpenRead(DeviceFileName);
                    using (BinaryReader binRead = new BinaryReader(fsDevFile))
                    {
                        //
                        DevFile.Info.VersionMajor = binRead.ReadInt32();
                        DevFile.Info.VersionMinor = binRead.ReadInt32();
                        DevFile.Info.VersionDot = binRead.ReadInt32();
                        DevFile.Info.VersionNotes = binRead.ReadString();
                        DevFile.Info.NumberFamilies = binRead.ReadInt32();
                        DevFile.Info.NumberParts = binRead.ReadInt32() + tblPartsList.Rows.Count;
                        DevFile.Info.NumberScripts = binRead.ReadInt32();
                        DevFile.Info.Compatibility = binRead.ReadByte();
                        DevFile.Info.UNUSED1A = binRead.ReadByte();
                        DevFile.Info.UNUSED1B = binRead.ReadUInt16();
                        DevFile.Info.UNUSED2 = binRead.ReadUInt32();

                        addTblInfo(tblInfo, DevFile.Info);

                        // create a version string
                        DeviceFileVersion = string.Format("{0:D1}.{1:D2}.{2:D2}", DevFile.Info.VersionMajor,
                                         DevFile.Info.VersionMinor, DevFile.Info.VersionDot);
                        //
                        // Declare arrays
                        //
                        DevFile.Families = new DeviceFile.DeviceFamilyParams[DevFile.Info.NumberFamilies];
                        DevFile.PartsList = new DeviceFile.DevicePartParams[DevFile.Info.NumberParts];
                        DevFile.Scripts = new DeviceFile.DeviceScripts[DevFile.Info.NumberScripts];

                        //
                        // now read all families if they are there
                        //
                        for (int l_x = 0; l_x < DevFile.Info.NumberFamilies; l_x++)
                        {
                            DevFile.Families[l_x].FamilyID = binRead.ReadUInt16();
                            DevFile.Families[l_x].FamilyType = binRead.ReadUInt16();
                            DevFile.Families[l_x].SearchPriority = binRead.ReadUInt16();
                            DevFile.Families[l_x].FamilyName = binRead.ReadString();
                            DevFile.Families[l_x].ProgEntryScript = binRead.ReadUInt16();
                            DevFile.Families[l_x].ProgExitScript = binRead.ReadUInt16();
                            DevFile.Families[l_x].ReadDevIDScript = binRead.ReadUInt16();
                            DevFile.Families[l_x].DeviceIDMask = binRead.ReadUInt32();
                            DevFile.Families[l_x].BlankValue = binRead.ReadUInt32();
                            DevFile.Families[l_x].BytesPerLocation = binRead.ReadByte();
                            DevFile.Families[l_x].AddressIncrement = binRead.ReadByte();
                            DevFile.Families[l_x].PartDetect = binRead.ReadBoolean();
                            DevFile.Families[l_x].ProgEntryVPPScript = binRead.ReadUInt16();
                            DevFile.Families[l_x].UNUSED1 = binRead.ReadUInt16();
                            DevFile.Families[l_x].EEMemBytesPerWord = binRead.ReadByte();
                            DevFile.Families[l_x].EEMemAddressIncrement = binRead.ReadByte();
                            DevFile.Families[l_x].UserIDHexBytes = binRead.ReadByte();
                            DevFile.Families[l_x].UserIDBytes = binRead.ReadByte();
                            DevFile.Families[l_x].ProgMemHexBytes = binRead.ReadByte();
                            DevFile.Families[l_x].EEMemHexBytes = binRead.ReadByte();
                            DevFile.Families[l_x].ProgMemShift = binRead.ReadByte();
                            DevFile.Families[l_x].TestMemoryStart = binRead.ReadUInt32();
                            DevFile.Families[l_x].TestMemoryLength = binRead.ReadUInt16();
                            DevFile.Families[l_x].Vpp = binRead.ReadSingle();

                            if (DevFile.Families[l_x].FamilyName == "PIC32" && tblPartsList.Rows.Count > 0)
                            { DevFile.Families[l_x].DeviceIDMask = 0xFFFF000; }
                        }

                        addTblFamilies(tblFamilies, DevFile.Families);

                        // Create family search table based on priority
                        familySearchTable = new int[DevFile.Info.NumberFamilies];
                        for (int familyIdx = 0; familyIdx < DevFile.Info.NumberFamilies; familyIdx++)
                        {
                            familySearchTable[DevFile.Families[familyIdx].SearchPriority] = familyIdx;
                        }
                        //
                        // now read all parts if they are there
                        //
                        int l_y = DevFile.Info.NumberParts - tblPartsList.Rows.Count;

                        for (int l_x = 0; l_x < l_y; l_x++)
                        {
                            DevFile.PartsList[l_x].PartName = binRead.ReadString();
                            DevFile.PartsList[l_x].Family = binRead.ReadUInt16();

                            if (tblPartsList.Rows.Count > 0 &&
                                DevFile.PartsList[l_x].PartName.Length >= 5 &&
                                DevFile.PartsList[l_x].PartName.Substring(0, 5) == "PIC32")
                            {   // timijk 2015.04.07 merge with PIC32MX support
                                DevFile.PartsList[l_x].PartName = "*" + DevFile.PartsList[l_x].PartName;
                                DevFile.PartsList[l_x].Family = 0xFFFF;   //disable original PIC32 parts
                            }
                            DevFile.PartsList[l_x].DeviceID = binRead.ReadUInt32();
                            DevFile.PartsList[l_x].ProgramMem = binRead.ReadUInt32();
                            DevFile.PartsList[l_x].EEMem = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].EEAddr = binRead.ReadUInt32();
                            DevFile.PartsList[l_x].ConfigWords = binRead.ReadByte();
                            DevFile.PartsList[l_x].ConfigAddr = binRead.ReadUInt32();
                            DevFile.PartsList[l_x].UserIDWords = binRead.ReadByte();
                            DevFile.PartsList[l_x].UserIDAddr = binRead.ReadUInt32();
                            DevFile.PartsList[l_x].BandGapMask = binRead.ReadUInt32();
                            // Init config arrays
                            DevFile.PartsList[l_x].ConfigMasks = new ushort[KONST.NumConfigMasks];
                            DevFile.PartsList[l_x].ConfigBlank = new ushort[KONST.NumConfigMasks];
                            for (int l_index = 0; l_index < KONST.MaxReadCfgMasks; l_index++)
                            {
                                DevFile.PartsList[l_x].ConfigMasks[l_index] = binRead.ReadUInt16();
                            }
                            for (int l_index = 0; l_index < KONST.MaxReadCfgMasks; l_index++)
                            {
                                DevFile.PartsList[l_x].ConfigBlank[l_index] = binRead.ReadUInt16();
                            }
                            DevFile.PartsList[l_x].CPMask = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].CPConfig = binRead.ReadByte();
                            DevFile.PartsList[l_x].OSSCALSave = binRead.ReadBoolean();
                            DevFile.PartsList[l_x].IgnoreAddress = binRead.ReadUInt32();
                            DevFile.PartsList[l_x].VddMin = binRead.ReadSingle();
                            DevFile.PartsList[l_x].VddMax = binRead.ReadSingle();
                            DevFile.PartsList[l_x].VddErase = binRead.ReadSingle();
                            DevFile.PartsList[l_x].CalibrationWords = binRead.ReadByte();
                            DevFile.PartsList[l_x].ChipEraseScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ProgMemAddrSetScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ProgMemAddrBytes = binRead.ReadByte();
                            DevFile.PartsList[l_x].ProgMemRdScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ProgMemRdWords = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].EERdPrepScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].EERdScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].EERdLocations = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].UserIDRdPrepScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].UserIDRdScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ConfigRdPrepScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ConfigRdScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ProgMemWrPrepScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ProgMemWrScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ProgMemWrWords = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ProgMemPanelBufs = binRead.ReadByte();
                            DevFile.PartsList[l_x].ProgMemPanelOffset = binRead.ReadUInt32();
                            DevFile.PartsList[l_x].EEWrPrepScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].EEWrScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].EEWrLocations = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].UserIDWrPrepScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].UserIDWrScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ConfigWrPrepScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ConfigWrScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].OSCCALRdScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].OSCCALWrScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DPMask = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].WriteCfgOnErase = binRead.ReadBoolean();
                            DevFile.PartsList[l_x].BlankCheckSkipUsrIDs = binRead.ReadBoolean();
                            DevFile.PartsList[l_x].IgnoreBytes = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ChipErasePrepScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].BootFlash = binRead.ReadUInt32();
                            //DevFile.PartsList[l_x].UNUSED4 = binRead.ReadUInt32();
                            DevFile.PartsList[l_x].Config9Mask = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ConfigMasks[8] = DevFile.PartsList[l_x].Config9Mask;
                            DevFile.PartsList[l_x].Config9Blank = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ConfigBlank[8] = DevFile.PartsList[l_x].Config9Blank;
                            DevFile.PartsList[l_x].ProgMemEraseScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].EEMemEraseScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ConfigMemEraseScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].reserved1EraseScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].reserved2EraseScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].TestMemoryRdScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].TestMemoryRdWords = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].EERowEraseScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].EERowEraseWords = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ExportToMPLAB = binRead.ReadBoolean();
                            DevFile.PartsList[l_x].DebugHaltScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugRunScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugStatusScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugReadExecVerScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugSingleStepScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugBulkWrDataScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugBulkRdDataScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugWriteVectorScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugReadVectorScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugRowEraseScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugRowEraseSize = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugReserved5Script = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugReserved6Script = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugReserved7Script = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugReserved8Script = binRead.ReadUInt16();
                            //DevFile.PartsList[l_x].DebugReserved9Script = binRead.ReadUInt16();                                                       
                            DevFile.PartsList[l_x].LVPScript = binRead.ReadUInt16();

                        }

                        addTblPartsList(tblPartsList, DevFile.PartsList);

                        //
                        // now read all scripts if they are there
                        //                    
                        for (int l_x = 0; l_x < DevFile.Info.NumberScripts; l_x++)
                        {
                            DevFile.Scripts[l_x].ScriptNumber = binRead.ReadUInt16();
                            DevFile.Scripts[l_x].ScriptName = binRead.ReadString();
                            DevFile.Scripts[l_x].ScriptVersion = binRead.ReadUInt16();
                            DevFile.Scripts[l_x].UNUSED1 = binRead.ReadUInt32();
                            DevFile.Scripts[l_x].ScriptLength = binRead.ReadUInt16();
                            // init script array
                            DevFile.Scripts[l_x].Script = new ushort[DevFile.Scripts[l_x].ScriptLength];
                            for (int l_index = 0; l_index < DevFile.Scripts[l_x].ScriptLength; l_index++)
                            {
                                DevFile.Scripts[l_x].Script[l_index] = binRead.ReadUInt16();
                            }
                            DevFile.Scripts[l_x].Comment = binRead.ReadString();

                        }

                        addTblScripts(tblScripts, DevFile.Scripts);

                        binRead.Close();
                    }
                    fsDevFile.Close();

                    //dsDeviceFile.WriteXmlSchema("c:\\temp\\dsDeviceFile.sch");

                    dsDeviceFile.WriteXml("c:\\temp\\dsDeviceFile.xml", System.Data.XmlWriteMode.WriteSchema);

                }
                catch
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }

        }

        public static bool ReadDeviceFile(string DeviceFileName)
        {
            string DeviceFileNameXML = DeviceFileName.Replace(".dat", ".xml");
            System.Data.DataSet dsDeviceFile;
            DTBL tblFamilies = new DTBL("Families");
            DTBL tblPartsList = new DTBL("PartsList");
            DTBL tblScripts = new DTBL("Scripts");

            if (File.Exists(DeviceFileNameXML))
            {
                try
                {
                    dsDeviceFile = new System.Data.DataSet("DeviceFile");
                    dsDeviceFile.ReadXml(DeviceFileNameXML, System.Data.XmlReadMode.ReadSchema);
                    tblFamilies = dsDeviceFile.Tables["Families"];
                    tblPartsList = dsDeviceFile.Tables["PartsList"];
                    tblScripts = dsDeviceFile.Tables["Scripts"];
                }
                catch
                {
                    return false;
                }
            }


            bool fileExists = File.Exists(DeviceFileName);
            //DTBL tblInfo = new DTBL("Info");
            //DTBL tblFamilies = new DTBL("Families");
            //DTBL tblPartsList = new DTBL("PartsList");
            //DTBL tblScripts = new DTBL("Scripts");

            //System.Data.DataSet dsDeviceFile = new System.Data.DataSet("DeviceFile");

            //dsDeviceFile.Tables.Add(tblInfo);
            //dsDeviceFile.Tables.Add(tblFamilies);
            //dsDeviceFile.Tables.Add(tblPartsList);
            //dsDeviceFile.Tables.Add(tblScripts);

            //setupDTBLs(tblInfo, tblFamilies, tblPartsList, tblScripts);

            if (fileExists)
            {
                try
                {
                    //FileStream fsDevFile = File.Open(DeviceFileName, FileMode.Open);
                    FileStream fsDevFile = File.OpenRead(DeviceFileName);
                    using (BinaryReader binRead = new BinaryReader(fsDevFile))
                    {
                        //
                        DevFile.Info.VersionMajor = binRead.ReadInt32();
                        DevFile.Info.VersionMinor = binRead.ReadInt32();
                        DevFile.Info.VersionDot = binRead.ReadInt32();
                        DevFile.Info.VersionNotes = binRead.ReadString();
                        DevFile.Info.NumberFamilies = binRead.ReadInt32();
                        DevFile.Info.NumberParts = binRead.ReadInt32() + tblPartsList.Rows.Count;
                        DevFile.Info.NumberScripts = binRead.ReadInt32() + tblScripts.Rows.Count;
                        DevFile.Info.Compatibility = binRead.ReadByte();
                        DevFile.Info.UNUSED1A = binRead.ReadByte();
                        DevFile.Info.UNUSED1B = binRead.ReadUInt16();
                        DevFile.Info.UNUSED2 = binRead.ReadUInt32();

                        //addTblInfo(tblInfo, DevFile.Info);

                        // create a version string
                        DeviceFileVersion = string.Format("{0:D1}.{1:D2}.{2:D2}", DevFile.Info.VersionMajor,
                                         DevFile.Info.VersionMinor, DevFile.Info.VersionDot);
                        //
                        // Declare arrays
                        //
                        DevFile.Families = new DeviceFile.DeviceFamilyParams[DevFile.Info.NumberFamilies];
                        DevFile.PartsList = new DeviceFile.DevicePartParams[DevFile.Info.NumberParts];
                        DevFile.Scripts = new DeviceFile.DeviceScripts[DevFile.Info.NumberScripts];

                        //
                        // now read all families if they are there
                        //
                        for (int l_x = 0; l_x < DevFile.Info.NumberFamilies; l_x++)
                        {
                            DevFile.Families[l_x].FamilyID = binRead.ReadUInt16();
                            DevFile.Families[l_x].FamilyType = binRead.ReadUInt16();
                            DevFile.Families[l_x].SearchPriority = binRead.ReadUInt16();
                            DevFile.Families[l_x].FamilyName = binRead.ReadString();
                            DevFile.Families[l_x].ProgEntryScript = binRead.ReadUInt16();
                            DevFile.Families[l_x].ProgExitScript = binRead.ReadUInt16();
                            DevFile.Families[l_x].ReadDevIDScript = binRead.ReadUInt16();
                            DevFile.Families[l_x].DeviceIDMask = binRead.ReadUInt32();
                            DevFile.Families[l_x].BlankValue = binRead.ReadUInt32();
                            DevFile.Families[l_x].BytesPerLocation = binRead.ReadByte();
                            DevFile.Families[l_x].AddressIncrement = binRead.ReadByte();
                            DevFile.Families[l_x].PartDetect = binRead.ReadBoolean();
                            DevFile.Families[l_x].ProgEntryVPPScript = binRead.ReadUInt16();
                            DevFile.Families[l_x].UNUSED1 = binRead.ReadUInt16();
                            DevFile.Families[l_x].EEMemBytesPerWord = binRead.ReadByte();
                            DevFile.Families[l_x].EEMemAddressIncrement = binRead.ReadByte();
                            DevFile.Families[l_x].UserIDHexBytes = binRead.ReadByte();
                            DevFile.Families[l_x].UserIDBytes = binRead.ReadByte();
                            DevFile.Families[l_x].ProgMemHexBytes = binRead.ReadByte();
                            DevFile.Families[l_x].EEMemHexBytes = binRead.ReadByte();
                            DevFile.Families[l_x].ProgMemShift = binRead.ReadByte();
                            DevFile.Families[l_x].TestMemoryStart = binRead.ReadUInt32();
                            DevFile.Families[l_x].TestMemoryLength = binRead.ReadUInt16();
                            DevFile.Families[l_x].Vpp = binRead.ReadSingle();

                            if (DevFile.Families[l_x].FamilyName == "PIC32" && tblPartsList.Rows.Count > 0)
                            { DevFile.Families[l_x].DeviceIDMask = 0xFFFF000; }
                        }

                        //timijk 2015.06.08
                        //replace the DevFile.Families if(tblFamilies.Rows.Count>0)
                        if (tblFamilies.Rows.Count > 0)
                        {
                            int l_x = 0;

                            DevFile.Info.NumberFamilies = tblFamilies.Rows.Count;

                            DevFile.Families = new DeviceFile.DeviceFamilyParams[DevFile.Info.NumberFamilies];

                            foreach (DataRow row in tblFamilies.Rows)
                            {
                                DevFile.Families[l_x].FamilyID = (UInt16)row["FamilyID"];
                                DevFile.Families[l_x].FamilyType = (UInt16)row["FamilyType"];
                                DevFile.Families[l_x].SearchPriority = (UInt16)row["SearchPriority"];
                                DevFile.Families[l_x].FamilyName = (String)row["FamilyName"];
                                DevFile.Families[l_x].ProgEntryScript = (UInt16)row["ProgEntryScript"];
                                DevFile.Families[l_x].ProgExitScript = (UInt16)row["ProgExitScript"];
                                DevFile.Families[l_x].ReadDevIDScript = (UInt16)row["ReadDevIDScript"];
                                DevFile.Families[l_x].DeviceIDMask = (UInt32)row["DeviceIDMask"];
                                DevFile.Families[l_x].BlankValue = (UInt32)row["BlankValue"];
                                DevFile.Families[l_x].BytesPerLocation = (Byte)row["BytesPerLocation"];
                                DevFile.Families[l_x].AddressIncrement = (Byte)row["AddressIncrement"];
                                DevFile.Families[l_x].PartDetect = (Boolean)row["PartDetect"];
                                DevFile.Families[l_x].ProgEntryVPPScript = (UInt16)row["ProgEntryVPPScript"];
                                DevFile.Families[l_x].UNUSED1 = (UInt16)row["UNUSED1"];
                                DevFile.Families[l_x].EEMemBytesPerWord = (Byte)row["EEMemBytesPerWord"];
                                DevFile.Families[l_x].EEMemAddressIncrement = (Byte)row["EEMemAddressIncrement"];
                                DevFile.Families[l_x].UserIDHexBytes = (Byte)row["UserIDHexBytes"];
                                DevFile.Families[l_x].UserIDBytes = (Byte)row["UserIDBytes"];
                                DevFile.Families[l_x].ProgMemHexBytes = (Byte)row["ProgMemHexBytes"];
                                DevFile.Families[l_x].EEMemHexBytes = (Byte)row["EEMemHexBytes"];
                                DevFile.Families[l_x].ProgMemShift = (Byte)row["ProgMemShift"];
                                DevFile.Families[l_x].TestMemoryStart = (UInt32)row["TestMemoryStart"];
                                DevFile.Families[l_x].TestMemoryLength = (UInt16)row["TestMemoryLength"];
                                DevFile.Families[l_x].Vpp = (Single)row["Vpp"];

                                if (DevFile.Families[l_x].FamilyName == "PIC32" && tblPartsList.Rows.Count > 0)
                                { DevFile.Families[l_x].DeviceIDMask = 0xFFFF000; }

                                l_x++;
                            }

                        }
                        //addTblFamilies(tblFamilies, DevFile.Families);

                        // Create family search table based on priority
                        familySearchTable = new int[DevFile.Info.NumberFamilies];
                        for (int familyIdx = 0; familyIdx < DevFile.Info.NumberFamilies; familyIdx++)
                        {
                            familySearchTable[DevFile.Families[familyIdx].SearchPriority] = familyIdx;
                        }
                        //
                        // now read all parts if they are there
                        //
                        int l_y = DevFile.Info.NumberParts - tblPartsList.Rows.Count;

                        for (int l_x = 0; l_x < l_y; l_x++)
                        {
                            DevFile.PartsList[l_x].PartName = binRead.ReadString();
                            DevFile.PartsList[l_x].Family = binRead.ReadUInt16();

                            if (tblPartsList.Rows.Count > 0 &&
                                DevFile.PartsList[l_x].PartName.Length >= 5 &&
                                DevFile.PartsList[l_x].PartName.Substring(0, 5) == "PIC32")
                            {
                                // timijk 2015.04.07 merge with PIC32MX support
                                DevFile.PartsList[l_x].PartName = "*" + DevFile.PartsList[l_x].PartName;
                                DevFile.PartsList[l_x].Family = 0xFFFF;   //disable original PIC32 parts
                            }
                            DevFile.PartsList[l_x].DeviceID = binRead.ReadUInt32();
                            DevFile.PartsList[l_x].ProgramMem = binRead.ReadUInt32();
                            DevFile.PartsList[l_x].EEMem = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].EEAddr = binRead.ReadUInt32();
                            DevFile.PartsList[l_x].ConfigWords = binRead.ReadByte();
                            DevFile.PartsList[l_x].ConfigAddr = binRead.ReadUInt32();
                            DevFile.PartsList[l_x].UserIDWords = binRead.ReadByte();
                            DevFile.PartsList[l_x].UserIDAddr = binRead.ReadUInt32();
                            DevFile.PartsList[l_x].BandGapMask = binRead.ReadUInt32();
                            // Init config arrays
                            DevFile.PartsList[l_x].ConfigMasks = new ushort[KONST.NumConfigMasks];
                            DevFile.PartsList[l_x].ConfigBlank = new ushort[KONST.NumConfigMasks];
                            for (int l_index = 0; l_index < KONST.MaxReadCfgMasks; l_index++)
                            {
                                DevFile.PartsList[l_x].ConfigMasks[l_index] = binRead.ReadUInt16();
                            }
                            for (int l_index = 0; l_index < KONST.MaxReadCfgMasks; l_index++)
                            {
                                DevFile.PartsList[l_x].ConfigBlank[l_index] = binRead.ReadUInt16();
                            }
                            DevFile.PartsList[l_x].CPMask = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].CPConfig = binRead.ReadByte();
                            DevFile.PartsList[l_x].OSSCALSave = binRead.ReadBoolean();
                            DevFile.PartsList[l_x].IgnoreAddress = binRead.ReadUInt32();
                            DevFile.PartsList[l_x].VddMin = binRead.ReadSingle();
                            DevFile.PartsList[l_x].VddMax = binRead.ReadSingle();
                            DevFile.PartsList[l_x].VddErase = binRead.ReadSingle();
                            DevFile.PartsList[l_x].CalibrationWords = binRead.ReadByte();
                            DevFile.PartsList[l_x].ChipEraseScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ProgMemAddrSetScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ProgMemAddrBytes = binRead.ReadByte();
                            DevFile.PartsList[l_x].ProgMemRdScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ProgMemRdWords = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].EERdPrepScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].EERdScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].EERdLocations = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].UserIDRdPrepScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].UserIDRdScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ConfigRdPrepScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ConfigRdScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ProgMemWrPrepScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ProgMemWrScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ProgMemWrWords = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ProgMemPanelBufs = binRead.ReadByte();
                            DevFile.PartsList[l_x].ProgMemPanelOffset = binRead.ReadUInt32();
                            DevFile.PartsList[l_x].EEWrPrepScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].EEWrScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].EEWrLocations = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].UserIDWrPrepScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].UserIDWrScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ConfigWrPrepScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ConfigWrScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].OSCCALRdScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].OSCCALWrScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DPMask = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].WriteCfgOnErase = binRead.ReadBoolean();
                            DevFile.PartsList[l_x].BlankCheckSkipUsrIDs = binRead.ReadBoolean();
                            DevFile.PartsList[l_x].IgnoreBytes = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ChipErasePrepScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].BootFlash = binRead.ReadUInt32();
                            //DevFile.PartsList[l_x].UNUSED4 = binRead.ReadUInt32();
                            DevFile.PartsList[l_x].Config9Mask = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ConfigMasks[8] = DevFile.PartsList[l_x].Config9Mask;
                            DevFile.PartsList[l_x].Config9Blank = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ConfigBlank[8] = DevFile.PartsList[l_x].Config9Blank;
                            DevFile.PartsList[l_x].ProgMemEraseScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].EEMemEraseScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ConfigMemEraseScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].reserved1EraseScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].reserved2EraseScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].TestMemoryRdScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].TestMemoryRdWords = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].EERowEraseScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].EERowEraseWords = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].ExportToMPLAB = binRead.ReadBoolean();
                            DevFile.PartsList[l_x].DebugHaltScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugRunScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugStatusScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugReadExecVerScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugSingleStepScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugBulkWrDataScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugBulkRdDataScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugWriteVectorScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugReadVectorScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugRowEraseScript = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugRowEraseSize = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugReserved5Script = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugReserved6Script = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugReserved7Script = binRead.ReadUInt16();
                            DevFile.PartsList[l_x].DebugReserved8Script = binRead.ReadUInt16();
                            //DevFile.PartsList[l_x].DebugReserved9Script = binRead.ReadUInt16();                                                       
                            DevFile.PartsList[l_x].LVPScript = binRead.ReadUInt16();

                        }

                        foreach (DataRow row in tblPartsList.Rows)
                        {
                            DevFile.PartsList[l_y].PartName = (String)row["PartName"];
                            DevFile.PartsList[l_y].Family = (UInt16)row["Family"];
                            DevFile.PartsList[l_y].DeviceID = (UInt32)row["DeviceID"] & DevFile.Families[DevFile.PartsList[l_y].Family].DeviceIDMask;
                            DevFile.PartsList[l_y].ProgramMem = (UInt32)row["ProgramMem"];
                            DevFile.PartsList[l_y].EEMem = (UInt16)row["EEMem"];
                            DevFile.PartsList[l_y].EEAddr = (UInt32)row["EEAddr"];
                            DevFile.PartsList[l_y].ConfigWords = (Byte)row["ConfigWords"];
                            DevFile.PartsList[l_y].ConfigAddr = (UInt32)row["ConfigAddr"];
                            DevFile.PartsList[l_y].UserIDWords = (Byte)row["UserIDWords"];
                            DevFile.PartsList[l_y].UserIDAddr = (UInt32)row["UserIDAddr"];
                            DevFile.PartsList[l_y].BandGapMask = (UInt32)row["BandGapMask"];
                            // Init config arrays
                            DevFile.PartsList[l_y].ConfigMasks = (ushort[])row["ConfigMasks"];
                            DevFile.PartsList[l_y].ConfigBlank = (ushort[])row["ConfigBlank"];
                            DevFile.PartsList[l_y].CPMask = (UInt16)row["CPMask"];
                            DevFile.PartsList[l_y].CPConfig = (Byte)row["CPConfig"];
                            DevFile.PartsList[l_y].OSSCALSave = (Boolean)row["OSSCALSave"];
                            DevFile.PartsList[l_y].IgnoreAddress = (UInt32)row["IgnoreAddress"];
                            DevFile.PartsList[l_y].VddMin = (Single)row["VddMin"];
                            DevFile.PartsList[l_y].VddMax = (Single)row["VddMax"];
                            DevFile.PartsList[l_y].VddErase = (Single)row["VddErase"];
                            DevFile.PartsList[l_y].CalibrationWords = (Byte)row["CalibrationWords"];
                            DevFile.PartsList[l_y].ChipEraseScript = (UInt16)row["ChipEraseScript"];
                            DevFile.PartsList[l_y].ProgMemAddrSetScript = (UInt16)row["ProgMemAddrSetScript"];
                            DevFile.PartsList[l_y].ProgMemAddrBytes = (Byte)row["ProgMemAddrBytes"];
                            DevFile.PartsList[l_y].ProgMemRdScript = (UInt16)row["ProgMemRdScript"];
                            DevFile.PartsList[l_y].ProgMemRdWords = (UInt16)row["ProgMemRdWords"];
                            DevFile.PartsList[l_y].EERdPrepScript = (UInt16)row["EERdPrepScript"];
                            DevFile.PartsList[l_y].EERdScript = (UInt16)row["EERdScript"];
                            DevFile.PartsList[l_y].EERdLocations = (UInt16)row["EERdLocations"];
                            DevFile.PartsList[l_y].UserIDRdPrepScript = (UInt16)row["UserIDRdPrepScript"];
                            DevFile.PartsList[l_y].UserIDRdScript = (UInt16)row["UserIDRdScript"];
                            DevFile.PartsList[l_y].ConfigRdPrepScript = (UInt16)row["ConfigRdPrepScript"];
                            DevFile.PartsList[l_y].ConfigRdScript = (UInt16)row["ConfigRdScript"];
                            DevFile.PartsList[l_y].ProgMemWrPrepScript = (UInt16)row["ProgMemWrPrepScript"];
                            DevFile.PartsList[l_y].ProgMemWrScript = (UInt16)row["ProgMemWrScript"];
                            DevFile.PartsList[l_y].ProgMemWrWords = (UInt16)row["ProgMemWrWords"];
                            DevFile.PartsList[l_y].ProgMemPanelBufs = (Byte)row["ProgMemPanelBufs"];
                            DevFile.PartsList[l_y].ProgMemPanelOffset = (UInt32)row["ProgMemPanelOffset"];
                            DevFile.PartsList[l_y].EEWrPrepScript = (UInt16)row["EEWrPrepScript"];
                            DevFile.PartsList[l_y].EEWrScript = (UInt16)row["EEWrScript"];
                            DevFile.PartsList[l_y].EEWrLocations = (UInt16)row["EEWrLocations"];
                            DevFile.PartsList[l_y].UserIDWrPrepScript = (UInt16)row["UserIDWrPrepScript"];
                            DevFile.PartsList[l_y].UserIDWrScript = (UInt16)row["UserIDWrScript"];
                            DevFile.PartsList[l_y].ConfigWrPrepScript = (UInt16)row["ConfigWrPrepScript"];
                            DevFile.PartsList[l_y].ConfigWrScript = (UInt16)row["ConfigWrScript"];
                            DevFile.PartsList[l_y].OSCCALRdScript = (UInt16)row["OSCCALRdScript"];
                            DevFile.PartsList[l_y].OSCCALWrScript = (UInt16)row["OSCCALWrScript"];
                            DevFile.PartsList[l_y].DPMask = (UInt16)row["DPMask"];
                            DevFile.PartsList[l_y].WriteCfgOnErase = (Boolean)row["WriteCfgOnErase"];
                            DevFile.PartsList[l_y].BlankCheckSkipUsrIDs = (Boolean)row["BlankCheckSkipUsrIDs"];
                            DevFile.PartsList[l_y].IgnoreBytes = (UInt16)row["IgnoreBytes"];
                            DevFile.PartsList[l_y].ChipErasePrepScript = (UInt16)row["ChipErasePrepScript"];
                            DevFile.PartsList[l_y].BootFlash = (UInt32)row["BootFlash"];
                            //DevFile.PartsList[l_y].UNUSED4 = (UInt32)row[""];
                            DevFile.PartsList[l_y].Config9Mask = (UInt16)row["Config9Mask"];
                            //DevFile.PartsList[l_y].ConfigMasks[8] = DevFile.PartsList[l_y].Config9Mask;
                            DevFile.PartsList[l_y].Config9Blank = (UInt16)row["Config9Blank"];
                            //DevFile.PartsList[l_y].ConfigBlank[8] = DevFile.PartsList[l_y].Config9Blank;
                            DevFile.PartsList[l_y].ProgMemEraseScript = (UInt16)row["ProgMemEraseScript"];
                            DevFile.PartsList[l_y].EEMemEraseScript = (UInt16)row["EEMemEraseScript"];
                            DevFile.PartsList[l_y].ConfigMemEraseScript = (UInt16)row["ConfigMemEraseScript"];
                            DevFile.PartsList[l_y].reserved1EraseScript = (UInt16)row["reserved1EraseScript"];
                            DevFile.PartsList[l_y].reserved2EraseScript = (UInt16)row["reserved2EraseScript"];
                            DevFile.PartsList[l_y].TestMemoryRdScript = (UInt16)row["TestMemoryRdScript"];
                            DevFile.PartsList[l_y].TestMemoryRdWords = (UInt16)row["TestMemoryRdWords"];
                            DevFile.PartsList[l_y].EERowEraseScript = (UInt16)row["EERowEraseScript"];
                            DevFile.PartsList[l_y].EERowEraseWords = (UInt16)row["EERowEraseWords"];
                            DevFile.PartsList[l_y].ExportToMPLAB = (Boolean)row["ExportToMPLAB"];
                            DevFile.PartsList[l_y].DebugHaltScript = (UInt16)row["DebugHaltScript"];
                            DevFile.PartsList[l_y].DebugRunScript = (UInt16)row["DebugRunScript"];
                            DevFile.PartsList[l_y].DebugStatusScript = (UInt16)row["DebugStatusScript"];
                            DevFile.PartsList[l_y].DebugReadExecVerScript = (UInt16)row["DebugReadExecVerScript"];
                            DevFile.PartsList[l_y].DebugSingleStepScript = (UInt16)row["DebugSingleStepScript"];
                            DevFile.PartsList[l_y].DebugBulkWrDataScript = (UInt16)row["DebugBulkWrDataScript"];
                            DevFile.PartsList[l_y].DebugBulkRdDataScript = (UInt16)row["DebugBulkRdDataScript"];
                            DevFile.PartsList[l_y].DebugWriteVectorScript = (UInt16)row["DebugWriteVectorScript"];
                            DevFile.PartsList[l_y].DebugReadVectorScript = (UInt16)row["DebugReadVectorScript"];
                            DevFile.PartsList[l_y].DebugRowEraseScript = (UInt16)row["DebugRowEraseScript"];
                            DevFile.PartsList[l_y].DebugRowEraseSize = (UInt16)row["DebugRowEraseSize"];
                            DevFile.PartsList[l_y].DebugReserved5Script = (UInt16)row["DebugReserved5Script"];
                            DevFile.PartsList[l_y].DebugReserved6Script = (UInt16)row["DebugReserved6Script"];
                            DevFile.PartsList[l_y].DebugReserved7Script = (UInt16)row["DebugReserved7Script"];
                            DevFile.PartsList[l_y].DebugReserved8Script = (UInt16)row["DebugReserved8Script"];
                            DevFile.PartsList[l_y].LVPScript = (UInt16)row["LVPScript"];

                            l_y++;
                        }

                        //addTblPartsList(tblPartsList, DevFile.PartsList);

                        //
                        // now read all scripts if they are there
                        //                    
                        l_y = DevFile.Info.NumberScripts - tblScripts.Rows.Count;
                        for (int l_x = 0; l_x < l_y; l_x++)
                        {
                            DevFile.Scripts[l_x].ScriptNumber = binRead.ReadUInt16();
                            DevFile.Scripts[l_x].ScriptName = binRead.ReadString();
                            DevFile.Scripts[l_x].ScriptVersion = binRead.ReadUInt16();
                            DevFile.Scripts[l_x].UNUSED1 = binRead.ReadUInt32();
                            DevFile.Scripts[l_x].ScriptLength = binRead.ReadUInt16();
                            // init script array
                            DevFile.Scripts[l_x].Script = new ushort[DevFile.Scripts[l_x].ScriptLength];
                            for (int l_index = 0; l_index < DevFile.Scripts[l_x].ScriptLength; l_index++)
                            {
                                DevFile.Scripts[l_x].Script[l_index] = binRead.ReadUInt16();
                            }
                            DevFile.Scripts[l_x].Comment = binRead.ReadString();

                        }

                        foreach (DataRow row in tblScripts.Rows)
                        {
                            DevFile.Scripts[l_y].ScriptNumber = (UInt16)row["ScriptNumber"];
                            DevFile.Scripts[l_y].ScriptName = (String)row["ScriptName"];
                            DevFile.Scripts[l_y].ScriptVersion = (UInt16)row["ScriptVersion"];
                            DevFile.Scripts[l_y].UNUSED1 = (UInt32)row["UNUSED1"];
                            DevFile.Scripts[l_y].ScriptLength = (UInt16)row["ScriptLength"];
                            DevFile.Scripts[l_y].Script = (ushort[])row["Script"];
                            DevFile.Scripts[l_y].Comment = (String)row["Comment"];

                            l_y++;
                        }
                        //addTblScripts(tblScripts, DevFile.Scripts);

                        binRead.Close();
                    }
                    fsDevFile.Close();

                    //dsDeviceFile.WriteXmlSchema("c:\\temp\\dsDeviceFile.sch");

                    //dsDeviceFile.WriteXml("c:\\temp\\dsDeviceFile.xml", System.Data.XmlWriteMode.WriteSchema);

                }
                catch
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }

        }

        public static bool DetectDevice(int familyIndex, bool resetOnNotFound, bool keepVddOn)
        {
            // Detect a device in the given Family of familyIndex, or all families
            Program.mCmdLogScripts.WriteLine("[DetectDevice:{0}]", familyIndex);

            if (familyIndex == KONST.SEARCH_ALL_FAMILIES)
            {
                // when searching all families, set Vdd = 3.3v
                if (!targetSelfPowered)
                { //but not if self-powered target
                    SetVDDVoltage(3.3F, 0.85F);
                }

                for (int searchIndex = 0; searchIndex < DevFile.Families.Length; searchIndex++)
                {
                    if (DevFile.Families[familySearchTable[searchIndex]].PartDetect)
                    {
                        if (searchDevice(familySearchTable[searchIndex], true, keepVddOn))
                        // 0 = no supported part found
                        {
                            return true;
                        }
                    }
                }
                return false; // no supported part found in any family
            }
            else
            {
                // reset VDD 
                SetVDDVoltage(vddLastSet, 0.85F);

                if (DevFile.Families[familyIndex].PartDetect)
                {
                    if (searchDevice(familyIndex, resetOnNotFound, keepVddOn))
                    {
                        return true;
                    }
                    return false;
                }
                else
                {
                    return true;    // don't fail unsearchable families like baseline.
                }
            }


        }

        public static int FindLastUsedInBuffer(uint[] bufferToSearch, uint blankValue,
                                                int startIndex)
        {   // go backawards from the start entry to find the last non-blank entry
            if (DevFile.Families[GetActiveFamily()].FamilyName != "KEELOQ?HCS")
            {
                for (int index = startIndex; index > 0; index--)
                {
                    if (bufferToSearch[index] != blankValue)
                    {
                        return index;
                    }
                }
            }
            else
            {
                return bufferToSearch.Length - 1;
            }

            return 0;
        }

        public static bool RunScriptUploadNoLen(int script, int repetitions)
        {
            // IMPORTANT NOTE: THIS ALWAYS CLEARS THE UPLOAD BUFFER FIRST!

            byte[] commandArray = new byte[5];
            commandArray[0] = KONST.CLR_UPLOAD_BUFFER;
            commandArray[1] = KONST.RUN_SCRIPT;
            commandArray[2] = scriptRedirectTable[script].redirectToScriptLocation;
            commandArray[3] = (byte)repetitions;
            commandArray[4] = KONST.UPLOAD_DATA_NOLEN;
            bool result = writeUSB(commandArray);
            //?timijk 2016.01.02 #issue
            //if (script == KONST.EE_RD) Thread.Sleep(200);
            if (result)
            {
                result = readUSB();
            }
            return result;
        }

        /* Deprecated for v2.60.00
        public static bool RunScriptUploadNoLen2(int script, int repetitions)
        {
            // IMPORTANT NOTE: THIS ALWAYS CLEARS THE UPLOAD BUFFER FIRST!

            byte[] commandArray = new byte[6];
            commandArray[0] = KONST.CLR_UPLOAD_BUFFER;
            commandArray[1] = KONST.RUN_SCRIPT;
            commandArray[2] = scriptRedirectTable[script].redirectToScriptLocation;
            commandArray[3] = (byte)repetitions;
            commandArray[4] = KONST.UPLOAD_DATA_NOLEN;
            commandArray[5] = KONST.UPLOAD_DATA_NOLEN;
            bool result = writeUSB(commandArray);
            if (result)
            {
                result = readUSB();
            }
            return result;
        } */

        public static bool GetUpload()
        {
            return readUSB();
        }

        public static bool UploadData()
        {
            byte[] commandArray = new byte[1];
            commandArray[0] = KONST.UPLOAD_DATA;
            bool result = writeUSB(commandArray);
            if (result)
            {
                result = readUSB();
            }
            return result;
        }

        public static bool UploadDataNoLen()
        {
            byte[] commandArray = new byte[1];
            commandArray[0] = KONST.UPLOAD_DATA_NOLEN;
            bool result = writeUSB(commandArray);
            if (result)
            {
                result = readUSB();
            }
            return result;
        }

        public static bool RunScript(int script, int repetitions)
        {
            // IMPORTANT NOTE: THIS ALWAYS CLEARS THE UPLOAD BUFFER FIRST!

            byte[] commandArray = new byte[4];
            commandArray[0] = KONST.CLR_UPLOAD_BUFFER;
            commandArray[1] = KONST.RUN_SCRIPT;
            commandArray[2] = scriptRedirectTable[script].redirectToScriptLocation;
            commandArray[3] = (byte)repetitions;
            if (writeUSB(commandArray))
            {
                if ((script == KONST.PROG_EXIT) && (!assertMCLR))
                {
                    return HoldMCLR(false);
                }

                //?timijk 2016.01.02 #issue
                //if (script == KONST.PROG_ENTRY) Thread.Sleep(200);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static int DataClrAndDownload(byte[] dataArray, int startIndex)
        // returns index of next byte to be transmitted. 0 = failed
        {
            if (startIndex >= dataArray.Length)
            {
                return 0;
            }
            int length = dataArray.Length - startIndex;
            if (length > 61)
            {
                length = 61;
            }
            byte[] commandArray = new byte[3 + length];
            commandArray[0] = KONST.CLR_DOWNLOAD_BUFFER;
            commandArray[1] = KONST.DOWNLOAD_DATA;
            commandArray[2] = (byte)(length & 0xFF);
            for (int i = 0; i < length; i++)
            {
                commandArray[3 + i] = dataArray[startIndex + i];
            }
            if (writeUSB(commandArray))
            {
                return (startIndex + length);
            }
            else
            {
                return 0;
            }
        }

        public static int DataDownload(byte[] dataArray, int startIndex, int lastIndex)
        // returns index of next byte to be transmitted. 0 = failed
        {
            if (startIndex >= lastIndex)
            {
                return 0;
            }
            int length = lastIndex - startIndex;
            if (length > 62)
            {
                length = 62;
            }
            byte[] commandArray = new byte[2 + length];
            commandArray[0] = KONST.DOWNLOAD_DATA;
            commandArray[1] = (byte)(length & 0xFF);
            for (int i = 0; i < length; i++)
            {
                commandArray[2 + i] = dataArray[startIndex + i];
            }
            if (writeUSB(commandArray))
            {
                return (startIndex + length);
            }
            else
            {
                return 0;
            }
        }

        public static bool DownloadAddress3(int address)
        {
            byte[] commandArray = new byte[6];
            commandArray[0] = KONST.CLR_DOWNLOAD_BUFFER;
            commandArray[1] = KONST.DOWNLOAD_DATA;
            commandArray[2] = 3;
            commandArray[3] = (byte)(address & 0xFF);
            commandArray[4] = (byte)(0xFF & (address >> 8));
            commandArray[5] = (byte)(0xFF & (address >> 16));
            return writeUSB(commandArray);
        }

        public static bool DownloadAddress3MSBFirst(int address)
        {
            byte[] commandArray = new byte[6];
            commandArray[0] = KONST.CLR_DOWNLOAD_BUFFER;
            commandArray[1] = KONST.DOWNLOAD_DATA;
            commandArray[2] = 3;
            commandArray[3] = (byte)(0xFF & (address >> 16));
            commandArray[4] = (byte)(0xFF & (address >> 8));
            commandArray[5] = (byte)(address & 0xFF);

            return writeUSB(commandArray);
        }

        public static bool Download3Multiples(int downloadBytes, int multiples, int increment)
        {
            byte firstCommand = KONST.CLR_DOWNLOAD_BUFFER;

            do
            {
                int thisWrite = multiples;
                if (multiples > 20) // can only write 20 per USB packet. (20 * 3 = 60 bytes)
                {
                    thisWrite = 20;
                    multiples -= 20;
                }
                else
                {
                    multiples = 0;
                }
                byte[] commandArray = new byte[(3 * thisWrite) + 3];
                commandArray[0] = firstCommand;
                commandArray[1] = KONST.DOWNLOAD_DATA;
                commandArray[2] = (byte)(3 * thisWrite);
                for (int i = 0; i < thisWrite; i++)
                {
                    commandArray[3 + (3 * i)] = (byte)(downloadBytes >> 16);
                    commandArray[4 + (3 * i)] = (byte)(downloadBytes >> 8);
                    commandArray[5 + (3 * i)] = (byte)downloadBytes;

                    downloadBytes += increment;
                }

                if (!writeUSB(commandArray))
                {
                    return false;
                }

                firstCommand = KONST.NO_OPERATION;
            } while (multiples > 0);

            return true;
        }

        public static uint ComputeChecksum(bool codeProtectOn, bool dataProtectOn)
        {
            uint checksum = 0;

            if (DevFile.Families[GetActiveFamily()].BlankValue < 0xFFFF)
            { // 16F and baseline parts are calculated a word at a time.
                // prog mem first
                int progMemEnd = (int)DevFile.PartsList[ActivePart].ProgramMem;

                if (DevFile.PartsList[ActivePart].OSSCALSave)
                { // don't include last location for devices with OSSCAL 
                    progMemEnd--;
                }

                if (DevFile.PartsList[ActivePart].ConfigWords > 0)
                {
                    if (((DevFile.PartsList[ActivePart].CPMask & DeviceBuffers.ConfigWords[DevFile.PartsList[ActivePart].CPConfig - 1])
                            != DevFile.PartsList[ActivePart].CPMask) || codeProtectOn)
                    {
                        if (DevFile.Families[GetActiveFamily()].BlankValue < 0x3FFF)
                        {
                            progMemEnd = 0x40; // BASELINE - last location of unprotected mem
                        }
                        else
                        {
                            progMemEnd = 0; // no memory included for midrange.
                        }
                    }
                }

                for (int idx = 0; idx < progMemEnd; idx++)
                {
                    checksum += DeviceBuffers.ProgramMemory[idx];
                }

                if (DevFile.PartsList[ActivePart].ConfigWords > 0)
                {
                    if (((DevFile.PartsList[ActivePart].CPMask & DeviceBuffers.ConfigWords[DevFile.PartsList[ActivePart].CPConfig - 1])
                            != DevFile.PartsList[ActivePart].CPMask) || codeProtectOn)
                    { // if a code protect bit is set, the checksum is computed differently.
                        //checksum = 0; // don't include memory (moved above)
                        for (int idx = 0; idx < DevFile.PartsList[ActivePart].UserIDWords; idx++)
                        { // add last nibble of UserIDs in decreasing nibble positions of checksum
                            int idPosition = 1;
                            for (int factor = 0; factor < idx; factor++)
                            {
                                idPosition <<= 4;
                            }
                            checksum += (uint)((0xF & DeviceBuffers.UserIDs[DevFile.PartsList[ActivePart].UserIDWords - idx - 1])
                                 * idPosition);
                        }
                    }

                    // config words
                    uint tempword = 0;
                    for (int idx = 0; idx < DevFile.PartsList[ActivePart].ConfigWords; idx++)
                    {
                        if (idx == (DevFile.PartsList[ActivePart].CPConfig - 1))
                        {
                            tempword = (DeviceBuffers.ConfigWords[idx] & DevFile.PartsList[ActivePart].ConfigMasks[idx]);
                            if (codeProtectOn)
                                tempword &= (uint)~DevFile.PartsList[ActivePart].CPMask;
                            if (dataProtectOn)
                                tempword &= (uint)~DevFile.PartsList[ActivePart].DPMask;
                            checksum += tempword;
                        }
                        else
                        {
                            checksum += (DeviceBuffers.ConfigWords[idx] & DevFile.PartsList[ActivePart].ConfigMasks[idx]);
                        }
                    }
                }
                return (checksum & 0xFFFF);
            }
            else
            { //PIC18, PIC24 are computed a byte at a time.
                int progMemEnd = (int)DevFile.PartsList[ActivePart].ConfigAddr / DevFile.Families[GetActiveFamily()].ProgMemHexBytes;
                if (progMemEnd > DevFile.PartsList[ActivePart].ProgramMem)
                {
                    progMemEnd = (int)DevFile.PartsList[ActivePart].ProgramMem;
                }

                for (int idx = 0; idx < progMemEnd; idx++)
                {
                    uint memWord = DeviceBuffers.ProgramMemory[idx];
                    checksum += (memWord & 0x000000FF);
                    for (int bite = 1; bite < DevFile.Families[GetActiveFamily()].BytesPerLocation; bite++)
                    {
                        memWord >>= 8;
                        checksum += (memWord & 0x000000FF);
                    }
                }

                if (DevFile.PartsList[ActivePart].ConfigWords > 0)
                {
                    if (((DevFile.PartsList[ActivePart].CPMask & DeviceBuffers.ConfigWords[DevFile.PartsList[ActivePart].CPConfig - 1])
                        != DevFile.PartsList[ActivePart].CPMask) || codeProtectOn)
                    { // if a code protect bit is set, the checksum is computed differently.
                        // NOTE: this will only match MPLAB checksum if ALL CP bits are set or ALL CP bits are clear.
                        checksum = 0; // don't include memory
                        for (int idx = 0; idx < DevFile.PartsList[ActivePart].UserIDWords; idx++)
                        { // add UserIDs to checksum
                            uint memWord = DeviceBuffers.UserIDs[idx];
                            checksum += (memWord & 0x000000FF);
                            checksum += ((memWord >> 8) & 0x000000FF);
                        }
                    }

                    // config words
                    for (int idx = 0; idx < DevFile.PartsList[ActivePart].ConfigWords; idx++)
                    {
                        uint memWord = (DeviceBuffers.ConfigWords[idx] & DevFile.PartsList[ActivePart].ConfigMasks[idx]);
                        checksum += (memWord & 0x000000FF);
                        checksum += ((memWord >> 8) & 0x000000FF);
                    }
                }
                return (checksum & 0xFFFF);
            }

        }

        public static void ResetBuffers()
        {
            DeviceBuffers = new DeviceData(DevFile.PartsList[ActivePart].ProgramMem,
                                            DevFile.PartsList[ActivePart].EEMem,
                                            DevFile.PartsList[ActivePart].ConfigWords,
                                            DevFile.PartsList[ActivePart].UserIDWords,
                                            DevFile.Families[GetActiveFamily()].BlankValue,
                                            DevFile.Families[GetActiveFamily()].EEMemAddressIncrement,
                                            DevFile.Families[GetActiveFamily()].UserIDBytes,
                                            DevFile.PartsList[ActivePart].ConfigBlank,
                                            DevFile.PartsList[ActivePart].ConfigMasks[KONST.OSCCAL_MASK]);
        }

        public static DeviceData CloneBuffers(DeviceData copyFrom)
        {
            DeviceData newBuffers = new DeviceData(DevFile.PartsList[ActivePart].ProgramMem,
                                            DevFile.PartsList[ActivePart].EEMem,
                                            DevFile.PartsList[ActivePart].ConfigWords,
                                            DevFile.PartsList[ActivePart].UserIDWords,
                                            DevFile.Families[GetActiveFamily()].BlankValue,
                                            DevFile.Families[GetActiveFamily()].EEMemAddressIncrement,
                                            DevFile.Families[GetActiveFamily()].UserIDBytes,
                                            DevFile.PartsList[ActivePart].ConfigBlank,
                                            DevFile.PartsList[ActivePart].ConfigMasks[KONST.OSCCAL_MASK]);

            // clone all the data
            for (int i = 0; i < copyFrom.ProgramMemory.Length; i++)
            {
                newBuffers.ProgramMemory[i] = copyFrom.ProgramMemory[i];
            }
            for (int i = 0; i < copyFrom.EEPromMemory.Length; i++)
            {
                newBuffers.EEPromMemory[i] = copyFrom.EEPromMemory[i];
            }
            for (int i = 0; i < copyFrom.ConfigWords.Length; i++)
            {
                newBuffers.ConfigWords[i] = copyFrom.ConfigWords[i];
            }
            for (int i = 0; i < copyFrom.UserIDs.Length; i++)
            {
                newBuffers.UserIDs[i] = copyFrom.UserIDs[i];
            }
            newBuffers.OSCCAL = copyFrom.OSCCAL;
            newBuffers.OSCCAL = copyFrom.BandGap;

            return newBuffers;
        }

        public static void PrepNewPart(bool resetBuffers)
        {
            if (resetBuffers)
                ResetBuffers();
            // Set VPP voltage by family
            float vpp = DevFile.Families[GetActiveFamily()].Vpp;
            if ((vpp < 1) || (lvpEnabled && (DevFile.PartsList[ActivePart].LVPScript > 0)))
            { // When nominally zero, use VDD voltage
                //UNLESS it's not an LVP script but a HV script (PIC24F-KA-)
                if (lvpEnabled && (DevFile.PartsList[ActivePart].LVPScript > 0))
                {
                    string scriptname = DevFile.Scripts[DevFile.PartsList[ActivePart].LVPScript - 1].ScriptName;
                    scriptname = scriptname.Substring(scriptname.Length - 2);
                    if (scriptname == "HV")
                    {
                        // the VPP voltage value is the 2nd script element in 100mV increments.
                        vpp = (float)DevFile.Scripts[DevFile.PartsList[ActivePart].LVPScript - 1].Script[1] / 10F;
                        SetVppVoltage(vpp, 0.7F);
                    }
                    else
                    {
                        SetVppVoltage(vddLastSet, 0.7F);
                    }
                }
                else
                {
                    SetVppVoltage(vddLastSet, 0.7F);
                }
            }
            else
            {
                SetVppVoltage(vpp, 0.7F);
            }
            downloadPartScripts(GetActiveFamily());
        }

        public static uint ReadDebugVector()
        {
            RunScript(KONST.PROG_ENTRY, 1);
            Program.mCmdLogScripts.WriteLine("[ExecuteScript:DebugReadVectorScript]");
            ExecuteScript(DevFile.PartsList[ActivePart].DebugReadVectorScript);
            UploadData();
            RunScript(KONST.PROG_EXIT, 1);
            int configWords = 2;
            int bufferIndex = 2;                    // report starts on index 1, which is #bytes uploaded.
            uint returnWords = 0;
            for (int word = 0; word < configWords; word++)
            {
                uint config = (uint)Usb_read_array[bufferIndex++];
                config |= (uint)Usb_read_array[bufferIndex++] << 8;
                if (DevFile.Families[GetActiveFamily()].ProgMemShift > 0)
                {
                    config = (config >> 1) & DevFile.Families[GetActiveFamily()].BlankValue;
                }
                if (word == 0)
                    returnWords = config;
                else
                    returnWords += (config << 16);
            }

            return returnWords;
        }

        public static void WriteDebugVector(uint debugWords)
        {
            int configWords = 2;
            byte[] configBuffer = new byte[4];

            RunScript(KONST.PROG_ENTRY, 1);

            for (int i = 0, j = 0; i < configWords; i++)
            {
                uint configWord = 0;
                if (i == 0)
                    configWord = (debugWords & 0xFFFF);
                else
                    configWord = (debugWords >> 16);
                if (DevFile.Families[GetActiveFamily()].ProgMemShift > 0)
                {
                    configWord = configWord << 1;
                }
                configBuffer[j++] = (byte)(configWord & 0xFF);
                configBuffer[j++] = (byte)((configWord >> 8) & 0xFF);
            }
            DataClrAndDownload(configBuffer, 0);
            Program.mCmdLogScripts.WriteLine("[ExecuteScript:DebugWriteVectorScript]");
            ExecuteScript(DevFile.PartsList[ActivePart].DebugWriteVectorScript);
            RunScript(KONST.PROG_EXIT, 1);

        }

        public static bool ReadPICkitVoltages(ref float vdd, ref float vpp)
        {
            byte[] commandArray = new byte[1];
            commandArray[0] = KONST.READ_VOLTAGES;
            if (writeUSB(commandArray))
            {
                if (readUSB())
                {
                    float valueADC = (float)((Usb_read_array[2] * 256) + Usb_read_array[1]);
                    vdd = (valueADC / 65536) * 5.0F;
                    valueADC = (float)((Usb_read_array[4] * 256) + Usb_read_array[3]);
                    vpp = (valueADC / 65536) * 13.7F;
                    return true;
                }
            }
            return false;
        }

        public static bool SetVppVoltage(float voltage, float threshold)
        {
            byte ccpValue = 0x40;
            byte vppADC = (byte)(voltage * 18.61F);
            byte vFault = (byte)(threshold * voltage * 18.61F);

            byte[] commandArray = new byte[4];
            commandArray[0] = KONST.SETVPP;
            commandArray[1] = ccpValue;
            commandArray[2] = vppADC;
            commandArray[3] = vFault;
            return writeUSB(commandArray);
        }


        public static bool SendScript(byte[] script)
        {
            int scriptLength = script.Length;

            byte[] commandArray = new byte[2 + scriptLength];
            commandArray[0] = KONST.EXECUTE_SCRIPT;
            commandArray[1] = (byte)scriptLength;
            for (int n = 0; n < scriptLength; n++)
            {
                commandArray[2 + n] = script[n];
            }
            return writeUSB(commandArray);
        }


        // ================================== PRIVATE METHODS ========================================

        private static ushort readPkStatus()
        {
            byte[] commandArray = new byte[1];
            commandArray[0] = KONST.READ_STATUS;
            if (writeUSB(commandArray))
            {
                if (readUSB())
                {
                    return (ushort)(Usb_read_array[2] * 256 + Usb_read_array[1]);
                }
                return 0xFFFF;
            }

            return 0xFFFF;
        }

        // timijk 2015.04.08 add logging flag, due to BL function not supported yet.. causing error
        public static bool writeUSB(byte[] commandList, bool logging = true)
        {
            if (spHandle != null) return writeBT(commandList);

            int bytesWritten = 0;

            //USB_BYTE_COUNT += commandList.Length;
            //USB_BYTE_COUNT++;

            Usb_write_array[0] = 0;                         // first byte must always be zero.        
            for (int index = 1; index < Usb_write_array.Length; index++)
            {
                Usb_write_array[index] = KONST.END_OF_BUFFER;              // init array to all END_OF_BUFFER cmds.
            }

            if (logging)
            {
                cmdList.processCmdList(Program.mCmdLogScripts, commandList);

                Program.mCmdLog.Write("00({0}):", commandList.Length.ToString("X2"));
                for (int ind = 0; ind < commandList.Length; ind++)
                {
                    Program.mCmdLog.Write((commandList[ind]).ToString("X2"));
                    Program.mCmdLog.Write(' ');
                }
                Program.mCmdLog.WriteLine();
            }

            Array.Copy(commandList, 0, Usb_write_array, 1, commandList.Length);

            //timijk 2016.01.22 performance tuning
            DateTime d = DateTime.Now;

            bool writeResult = USB.WriteFile(usbWriteHandle, Usb_write_array, Usb_write_array.Length, ref bytesWritten, 0);
            if (bytesWritten != Usb_write_array.Length)
            {
                return false;
            }

            TimeSpan diff = DateTime.Now - d;

            writeBT_cnt++;
            writeBT_acc_response_time2 += diff.TotalMilliseconds;

            double avg = writeBT_acc_response_time2 / (double)writeBT_cnt;

            if (writeBT_cnt == 512)
            {
                avg = avg;
            }

            return writeResult;

        }

        public static bool readUSB()
        {
            if (spHandle != null) return readBT();

            int bytesRead = 0;

            if (LearnMode)
                return true;

            bool readResult = USB.ReadFile(usbReadHandle, Usb_read_array, Usb_read_array.Length, ref bytesRead, 0);
            if (bytesRead != Usb_read_array.Length)
            {
                return false;
            }
            Program.mCmdLog.Write("01({0}):", Usb_read_array.Length.ToString("X2"));
            Program.mCmdLogScripts.WriteLine("01({0}):", Usb_read_array.Length.ToString("X2"));
            for (int ind = 0; ind < Usb_read_array.Length; ind++)
            {
                Program.mCmdLog.Write((Usb_read_array[ind]).ToString("X2"));
                Program.mCmdLog.Write(' ');
            }
            Program.mCmdLog.WriteLine();
            Program.mCmdLogScripts.WriteLine();

            return readResult;

        }

        public static bool VerifyDeviceID(bool resetOnNoDevice, bool keepVddOn)
        {
            // NOTE: the interface portion should ensure that self-powered targets
            // are detected before calling this function.

            // Set VPP voltage by family
            float vpp = DevFile.Families[GetActiveFamily()].Vpp;
            if ((vpp < 1) || (lvpEnabled && (DevFile.PartsList[ActivePart].LVPScript > 0)))
            { // When nominally zero, use VDD voltage
                //UNLESS it's not an LVP script but a HV script (PIC24F-KA-)
                if (lvpEnabled && (DevFile.PartsList[ActivePart].LVPScript > 0))
                {
                    string scriptname = DevFile.Scripts[DevFile.PartsList[ActivePart].LVPScript - 1].ScriptName;
                    scriptname = scriptname.Substring(scriptname.Length - 2);
                    if (scriptname == "HV")
                    {
                        // the VPP voltage value is the 2nd script element in 100mV increments.
                        vpp = (float)DevFile.Scripts[DevFile.PartsList[ActivePart].LVPScript - 1].Script[1] / 10F;
                        SetVppVoltage(vpp, 0.7F);
                    }
                    else
                    {
                        SetVppVoltage(vddLastSet, 0.7F);
                    }
                }
                else
                {
                    SetVppVoltage(vddLastSet, 0.7F);
                }
            }
            else
            {
                SetVppVoltage(vpp, 0.7F);
            }

            // Turn on Vdd (if self-powered, just turns off ground resistor)
            SetMCLRTemp(true);     // assert /MCLR to prevent code execution before programming mode entered.
            VddOn();

            // use direct execute scripts when checking for a part
            if (lvpEnabled && (DevFile.PartsList[ActivePart].LVPScript > 0))
            {
                Program.mCmdLogScripts.WriteLine("[ExecuteScript:LVPScript]");
                ExecuteScript(DevFile.PartsList[ActivePart].LVPScript);
            }
            else if (vppFirstEnabled && (DevFile.Families[GetActiveFamily()].ProgEntryVPPScript > 0))
            {
                Program.mCmdLogScripts.WriteLine("[ExecuteScript:ProgEntryVPPScript]");
                ExecuteScript(DevFile.Families[GetActiveFamily()].ProgEntryVPPScript);
            }
            else
            {
                Program.mCmdLogScripts.WriteLine("[ExecuteScript:ProgEntryScript]");
                ExecuteScript(DevFile.Families[GetActiveFamily()].ProgEntryScript);
            }
            Program.mCmdLogScripts.WriteLine("[ExecuteScript:ReadDevIDScript]");
            ExecuteScript(DevFile.Families[GetActiveFamily()].ReadDevIDScript);
            UploadData();
            Program.mCmdLogScripts.WriteLine("[ExecuteScript:ProgExitScript]");
            ExecuteScript(DevFile.Families[GetActiveFamily()].ProgExitScript);

            // Turn off Vdd (if PICkit-powered, turns on ground resistor)
            if (!keepVddOn)
            { // don't want it off when user wants PICkit 2 VDD "ON"
                VddOff();
            }

            if (!assertMCLR)
            {
                HoldMCLR(false);
            }

            uint deviceID;

            //timijk 2016.01.17
            if (DevFile.Families[GetActiveFamily()].FamilyName == "Midrange/1.8V Min+")
            {
                deviceID = (uint)(Usb_read_array[5] * 0x100 + Usb_read_array[4]);
                LastDeviceRev = (int)(Usb_read_array[3] * 0x100 + Usb_read_array[2]);
            }
            else
            {
                // NOTE: parts that only return 2 bytes for DevID will have junk in upper word.  This is OK - it gets masked off
                deviceID = (uint)(Usb_read_array[5] * 0x1000000 + Usb_read_array[4] * 0x10000 + Usb_read_array[3] * 256 + Usb_read_array[2]);
            }

            for (int shift = 0; shift < DevFile.Families[GetActiveFamily()].ProgMemShift; shift++)
            {
                deviceID >>= 1;         // midrange/baseline part results must be shifted by 1
                LastDeviceRev >>= 1;
            }

            if (DevFile.Families[GetActiveFamily()].FamilyName != "Midrange/1.8V Min+")
            {
                if (Usb_read_array[1] == 4) // 16-bit/32-bit parts have Rev in separate word
                {
                    LastDeviceRev = (int)(Usb_read_array[5] * 256 + Usb_read_array[4]);
                    if (DevFile.Families[GetActiveFamily()].BlankValue == 0xFFFFFFFF) // PIC32
                        LastDeviceRev >>= 4;
                }
                else
                    LastDeviceRev = (int)(deviceID & ~DevFile.Families[GetActiveFamily()].DeviceIDMask);
                LastDeviceRev &= 0xFFFF; // make sure to clear upper word.
                LastDeviceRev &= (int)DevFile.Families[GetActiveFamily()].BlankValue;
            }

            deviceID &= DevFile.Families[GetActiveFamily()].DeviceIDMask; // mask off version bits.
            LastDeviceID = deviceID;

            if (deviceID != DevFile.PartsList[ActivePart].DeviceID)
            {
                return false;
            }

            // Get OSCCAL if exists
            if (DevFile.PartsList[ActivePart].OSSCALSave)
            {
                VddOn();
                ReadOSSCAL();
            }
            if (DevFile.PartsList[ActivePart].BandGapMask > 0)
            {
                VddOn();
                ReadBandGap();
            }
            if (!keepVddOn)
            {
                VddOff();
            }

            return true;
        }


        private static bool searchDevice(int familyIndex, bool resetOnNoDevice, bool keepVddOn)
        {
            Program.mCmdLogScripts.WriteLine("[searchDevice:{0}]", familyIndex);

            //1.8V Min+
            //if (familyIndex != 0x13) return false;

            //PIC32MX 
            //if (familyIndex != 0x10) return false;

            //PIC32MM 
            if (familyIndex != 0x14) return false;

            //PIC18F_K_ if (familyIndex != 0x06) return false;
            //XXyy timijk 2015.06.08 dsPIC33EP 
            //if (familyIndex != 18) return false;

            int lastPart = ActivePart;  // remember the current part
            if (ActivePart != 0)
            {
                lastFoundPart = ActivePart;
            }

            // NOTE: the interface portion should ensure that self-powered targets
            // are detected before calling this function.

            // Set VPP voltage by family
            float vpp = DevFile.Families[familyIndex].Vpp;
            if ((vpp < 1) || (lvpEnabled && (DevFile.PartsList[ActivePart].LVPScript > 0)))
            { // When nominally zero, use VDD voltage
                //UNLESS it's not an LVP script but a HV script (PIC24F-KA-)
                if (lvpEnabled && (DevFile.PartsList[ActivePart].LVPScript > 0))
                {
                    string scriptname = DevFile.Scripts[DevFile.PartsList[ActivePart].LVPScript - 1].ScriptName;
                    scriptname = scriptname.Substring(scriptname.Length - 2);
                    if (scriptname == "HV")
                    {
                        // the VPP voltage value is the 2nd script element in 100mV increments.
                        vpp = (float)DevFile.Scripts[DevFile.PartsList[ActivePart].LVPScript - 1].Script[1] / 10F;
                        SetVppVoltage(vpp, 0.7F);
                    }
                    else
                    {
                        SetVppVoltage(vddLastSet, 0.7F);
                    }
                }
                else
                {
                    SetVppVoltage(vddLastSet, 0.7F);
                }
            }
            else
            {
                SetVppVoltage(vpp, 0.7F);
            }

            // Turn on Vdd (if self-powered, just turns off ground resistor)
            SetMCLRTemp(true);     // assert /MCLR to prevent code execution before programming mode entered.
            VddOn();

            // use direct execute scripts when checking for a part
            if (lvpEnabled && (DevFile.PartsList[ActivePart].LVPScript > 0))
            {
                Program.mCmdLogScripts.WriteLine("[ExecuteScript:LVPScript]");
                ExecuteScript(DevFile.PartsList[ActivePart].LVPScript);
            }
            else if (vppFirstEnabled && (DevFile.Families[familyIndex].ProgEntryVPPScript > 0))
            {
                Program.mCmdLogScripts.WriteLine("[ExecuteScript:ProgEntryVPPScript]");
                ExecuteScript(DevFile.Families[familyIndex].ProgEntryVPPScript);
            }
            else
            {
                Program.mCmdLogScripts.WriteLine("[ExecuteScript:ProgEntryScript]");
                ExecuteScript(DevFile.Families[familyIndex].ProgEntryScript);
            }

            Program.mCmdLogScripts.WriteLine("[ExecuteScript:ReadDevIDScript]");

            ExecuteScript(DevFile.Families[familyIndex].ReadDevIDScript);

            //?timijk 2016.01.01 #issue
            //timijk 2016.01.02 #issue solved by waiting for a complete signal
            //Thread.Sleep(160);

            UploadData();

            Program.mCmdLogScripts.WriteLine("[ExecuteScript:ProgExitScript]");
            ExecuteScript(DevFile.Families[familyIndex].ProgExitScript);

            //?timijk 2016.01.01 #issue
            //Thread.Sleep(160);

            // Turn off Vdd (if PICkit-powered, turns on ground resistor)
            if (!keepVddOn)
            { // don't want it off when user wants PICkit 2 VDD "ON"
                VddOff();
            }

            if (!assertMCLR)
            {
                HoldMCLR(false);
            }

            uint deviceID;

            //timijk 2016.01.17
            if (DevFile.Families[familyIndex].FamilyName == "Midrange/1.8V Min+")
            {
                deviceID = (uint)(Usb_read_array[5] * 0x100 + Usb_read_array[4]);
                LastDeviceRev = (int)(Usb_read_array[3] * 0x100 + Usb_read_array[2]);
            }
            else
            {
                // NOTE: parts that only return 2 bytes for DevID will have junk in upper word.  This is OK - it gets masked off
                deviceID = (uint)(Usb_read_array[5] * 0x1000000 + Usb_read_array[4] * 0x10000 + Usb_read_array[3] * 256 + Usb_read_array[2]);
            }

            for (int shift = 0; shift < DevFile.Families[familyIndex].ProgMemShift; shift++)
            {
                deviceID >>= 1;         // midrange/baseline part results must be shifted by 1
                LastDeviceRev >>= 1;
            }

            if (DevFile.Families[familyIndex].FamilyName != "Midrange/1.8V Min+")
            {
                if (Usb_read_array[1] == 4) // 16-bit/32-bit parts have Rev in separate word
                {
                    LastDeviceRev = (int)(Usb_read_array[5] * 256 + Usb_read_array[4]);
                    if (DevFile.Families[familyIndex].BlankValue == 0xFFFFFFFF) // PIC32
                        LastDeviceRev >>= 4;
                }
                else
                    LastDeviceRev = (int)(deviceID & ~DevFile.Families[familyIndex].DeviceIDMask);
                LastDeviceRev &= 0xFFFF; // make sure to clear upper word.
                LastDeviceRev &= (int)DevFile.Families[familyIndex].BlankValue;
            }

            deviceID &= DevFile.Families[familyIndex].DeviceIDMask; // mask off version bits.
            LastDeviceID = deviceID;

            if (deviceID == 0x06B12000)   //timijk PIC32MM0064GPL028
            {
                LastDeviceID = deviceID;
                LastDeviceRev = LastDeviceRev >> 8;
            }

            // Search through the device file to see if we find the part
            ActivePart = 0; // no device is default.
            for (int partEntry = 0; partEntry < DevFile.PartsList.Length; partEntry++)
            {
                if (DevFile.PartsList[partEntry].Family == familyIndex)
                { // don't check device ID if in a different family
                    if (DevFile.PartsList[partEntry].DeviceID == deviceID)
                    {
                        ActivePart = partEntry;
                        //xxyy timijk 2015.06.10
                        if (FamilyIsdsPIC30())
                        { ActivePICmicro = new dsPIC33_PE(); }
                        else if (FamilyIsdsPIC33EP())
                        { ActivePICmicro = new dsPIC33EP_PE(); }
                        else if (FamilyIsPIC24FJ())
                        { ActivePICmicro = new PIC24F_PE(); }
                        else
                        { ActivePICmicro = new PICmicro(); }

                        break;  // found a match - get out of the loop.
                    }
                }
            }

            if (ActivePart == 0) // not a known part
            {   // still need a buffer object in existance.
                if (lastPart != 0)
                {
                    DevFile.PartsList[ActivePart] = DevFile.PartsList[lastPart];
                    DevFile.PartsList[ActivePart].DeviceID = 0;
                    DevFile.PartsList[ActivePart].PartName = "Unsupported Part";
                }
                if (resetOnNoDevice)
                {
                    ResetBuffers();
                }
                return false;   // we're done
            }

            //if ((ActivePart == lastPart) && (scriptBufferChecksum == getScriptBufferChecksum())) 
            //{// same part we have been using (ensure scipt buffer hasn't been corrupted.)
            //    return true;    // don't need to download scripts as they should already be there.
            //}

            if ((ActivePart == lastFoundPart) && (scriptBufferChecksum != 0)
                        && (scriptBufferChecksum == getScriptBufferChecksum()))
            {// same as the last part we were connected to.
                Program.mCmdLogScripts.WriteLine("[searchDevice:{0}] Device is found", familyIndex);
                return true;    // don't need to download scripts as they should already be there.
            }

            // Getting here means we've found a part, but it's a new one so we need to download scripts
            downloadPartScripts(familyIndex);

            // create a new set of device buffers
            // If only need to redownload scripts, don't clear buffer.
            if (ActivePart != lastFoundPart)
            {
                ResetBuffers();
            }

            // Get OSCCAL if exists
            if (DevFile.PartsList[ActivePart].OSSCALSave)
            {
                VddOn();
                ReadOSSCAL();
            }
            if (DevFile.PartsList[ActivePart].BandGapMask > 0)
            {
                VddOn();
                ReadBandGap();
            }
            if (!keepVddOn)
            {
                VddOff();
            }

            Program.mCmdLogScripts.WriteLine("[searchDevice:{0}] Device is found", familyIndex);
            return true;

        }


        private static void downloadPartScripts(int familyIndex)
        {
            //timijk 2015.06.06
            //if (false) // FamilyIsdsPIC33EP())
            //{
            //    // timijk 2015.06.08
            //    //DevFile.PartsList[ActivePart].ProgramMem = 2048;

            //    dsPIC33EP_DevScr.init();

            //    dsPIC33EP_DevScr.ProgMemAddrSetScript.ScriptNumber = DevFile.PartsList[ActivePart].ProgMemAddrSetScript;
            //    DevFile.Scripts[DevFile.PartsList[ActivePart].ProgMemAddrSetScript - 1] = dsPIC33EP_DevScr.ProgMemAddrSetScript;

            //    dsPIC33EP_DevScr.ProgMemRdScript.ScriptNumber = DevFile.PartsList[ActivePart].ProgMemRdScript;
            //    DevFile.Scripts[DevFile.PartsList[ActivePart].ProgMemRdScript - 1] = dsPIC33EP_DevScr.ProgMemRdScript;

            //    dsPIC33EP_DevScr.ChipEraseScript.ScriptNumber = DevFile.PartsList[ActivePart].ChipEraseScript;
            //    DevFile.Scripts[DevFile.PartsList[ActivePart].ChipEraseScript - 1] = dsPIC33EP_DevScr.ChipEraseScript;

            //    dsPIC33EP_DevScr.ConfigRdScript.ScriptNumber = DevFile.PartsList[ActivePart].ConfigRdScript;
            //    DevFile.Scripts[DevFile.PartsList[ActivePart].ConfigRdScript - 1] = dsPIC33EP_DevScr.ConfigRdScript;

            //    dsPIC33EP_DevScr.UserIDRdScript.ScriptNumber = DevFile.PartsList[ActivePart].UserIDRdScript;
            //    DevFile.Scripts[DevFile.PartsList[ActivePart].UserIDRdScript - 1] = dsPIC33EP_DevScr.UserIDRdScript;


            //    dsPIC33EP_DevScr.ProgMemWrPrepScript.ScriptNumber = DevFile.PartsList[ActivePart].ProgMemWrPrepScript;
            //    DevFile.Scripts[DevFile.PartsList[ActivePart].ProgMemWrPrepScript - 1] = dsPIC33EP_DevScr.ProgMemWrPrepScript;

            //    dsPIC33EP_DevScr.ProgMemWrScript.ScriptNumber = DevFile.PartsList[ActivePart].ProgMemWrScript;
            //    DevFile.Scripts[DevFile.PartsList[ActivePart].ProgMemWrScript - 1] = dsPIC33EP_DevScr.ProgMemWrScript;

            //    dsPIC33EP_DevScr.ConfigWrScript.ScriptNumber = DevFile.PartsList[ActivePart].ConfigWrScript;
            //    DevFile.Scripts[DevFile.PartsList[ActivePart].ConfigWrScript - 1] = dsPIC33EP_DevScr.ConfigWrScript;

            //    dsPIC33EP_DevScr.DebugWriteVectorScript.ScriptNumber = DevFile.PartsList[ActivePart].DebugWriteVectorScript;
            //    DevFile.Scripts[DevFile.PartsList[ActivePart].DebugWriteVectorScript - 1] = dsPIC33EP_DevScr.DebugWriteVectorScript;

            //    dsPIC33EP_DevScr.exportXML();
            //}

            byte[] commandArray = new byte[1];
            commandArray[0] = KONST.CLR_SCRIPT_BUFFER;      // clear script buffer- we're loading new scripts
            bool result = writeUSB(commandArray);

            // clear the script redirect table
            for (int i = 0; i < scriptRedirectTable.Length; i++)
            {
                scriptRedirectTable[i].redirectToScriptLocation = 0;
                scriptRedirectTable[i].deviceFileScriptNumber = 0;
            }

            // program entry
            if (DevFile.Families[familyIndex].ProgEntryScript != 0) // don't download non-existant scripts
            {
                if (lvpEnabled && (DevFile.PartsList[ActivePart].LVPScript > 0))
                {
                    Program.mCmdLogScripts.WriteLine("[#LVPScript]");
                    downloadScript(KONST.PROG_ENTRY, DevFile.PartsList[ActivePart].LVPScript);
                }
                else if (vppFirstEnabled && (DevFile.Families[familyIndex].ProgEntryVPPScript != 0))
                { // download VPP first program mode entry
                    Program.mCmdLogScripts.WriteLine("[#ProgEntryVPPScript]");
                    downloadScript(KONST.PROG_ENTRY, DevFile.Families[familyIndex].ProgEntryVPPScript);
                }
                else
                { // standard program entry
                    Program.mCmdLogScripts.WriteLine("[#ProgEntryScript]");
                    downloadScript(KONST.PROG_ENTRY, DevFile.Families[familyIndex].ProgEntryScript);
                }
            }
            // program exit
            if (DevFile.Families[familyIndex].ProgExitScript != 0) // don't download non-existant scripts
            {
                Program.mCmdLogScripts.WriteLine("[#ProgExitScript]");
                downloadScript(KONST.PROG_EXIT, DevFile.Families[familyIndex].ProgExitScript);
            }
            // read device id
            if (DevFile.Families[familyIndex].ReadDevIDScript != 0) // don't download non-existant scripts
            {
                Program.mCmdLogScripts.WriteLine("[#ReadDevIDScript]");
                downloadScript(KONST.RD_DEVID, DevFile.Families[familyIndex].ReadDevIDScript);
            }
            // read program memory
            if (DevFile.PartsList[ActivePart].ProgMemRdScript != 0) // don't download non-existant scripts
            {
                Program.mCmdLogScripts.WriteLine("[#ProgMemRdScript]");
                downloadScript(KONST.PROGMEM_RD, DevFile.PartsList[ActivePart].ProgMemRdScript);
            }
            // chip erase prep
            if (DevFile.PartsList[ActivePart].ChipErasePrepScript != 0) // don't download non-existant scripts
            {
                Program.mCmdLogScripts.WriteLine("[#ChipErasePrepScript]");
                downloadScript(KONST.ERASE_CHIP_PREP, DevFile.PartsList[ActivePart].ChipErasePrepScript);
            }
            // set program memory address
            if (DevFile.PartsList[ActivePart].ProgMemAddrSetScript != 0) // don't download non-existant scripts
            {
                Program.mCmdLogScripts.WriteLine("[#ProgMemAddrSetScript]");
                downloadScript(KONST.PROGMEM_ADDRSET, DevFile.PartsList[ActivePart].ProgMemAddrSetScript);
            }
            // prepare for program memory write
            if (DevFile.PartsList[ActivePart].ProgMemWrPrepScript != 0) // don't download non-existant scripts
            {
                Program.mCmdLogScripts.WriteLine("[#ProgMemWrPrepScript]");
                downloadScript(KONST.PROGMEM_WR_PREP, DevFile.PartsList[ActivePart].ProgMemWrPrepScript);
            }
            // program memory write                 
            if (DevFile.PartsList[ActivePart].ProgMemWrScript != 0) // don't download non-existant scripts
            {
                Program.mCmdLogScripts.WriteLine("[#ProgMemWrScript]");
                downloadScript(KONST.PROGMEM_WR, DevFile.PartsList[ActivePart].ProgMemWrScript);
                //timijk: not required if (FamilyIsdsPIC33EP())  
                //{
                //    downloadScript(KONST.PROGMEM_WR_AUX, 280);
                //}
            }
            // prep for ee read               
            if (DevFile.PartsList[ActivePart].EERdPrepScript != 0) // don't download non-existant scripts
            {
                Program.mCmdLogScripts.WriteLine("[#EERdPrepScript]");
                downloadScript(KONST.EE_RD_PREP, DevFile.PartsList[ActivePart].EERdPrepScript);
            }
            // ee read               
            if (DevFile.PartsList[ActivePart].EERdScript != 0) // don't download non-existant scripts
            {
                Program.mCmdLogScripts.WriteLine("[#EERdScript]");
                downloadScript(KONST.EE_RD, DevFile.PartsList[ActivePart].EERdScript);
            }
            // prep for ee write               
            if (DevFile.PartsList[ActivePart].EEWrPrepScript != 0) // don't download non-existant scripts
            {
                Program.mCmdLogScripts.WriteLine("[#EEWrPrepScript]");
                downloadScript(KONST.EE_WR_PREP, DevFile.PartsList[ActivePart].EEWrPrepScript);
            }
            // ee write               
            if (DevFile.PartsList[ActivePart].EEWrScript != 0) // don't download non-existant scripts
            {
                Program.mCmdLogScripts.WriteLine("[#EEWrScript]");
                downloadScript(KONST.EE_WR, DevFile.PartsList[ActivePart].EEWrScript);
            }
            // prep for config read       
            if (DevFile.PartsList[ActivePart].ConfigRdPrepScript != 0) // don't download non-existant scripts
            {
                Program.mCmdLogScripts.WriteLine("[#ConfigRdPrepScript]");
                downloadScript(KONST.CONFIG_RD_PREP, DevFile.PartsList[ActivePart].ConfigRdPrepScript);
            }
            // config read       
            if (DevFile.PartsList[ActivePart].ConfigRdScript != 0) // don't download non-existant scripts
            {
                Program.mCmdLogScripts.WriteLine("[#ConfigRdScript]");
                downloadScript(KONST.CONFIG_RD, DevFile.PartsList[ActivePart].ConfigRdScript);
            }
            // prep for config write       
            if (DevFile.PartsList[ActivePart].ConfigWrPrepScript != 0) // don't download non-existant scripts
            {
                Program.mCmdLogScripts.WriteLine("[#ConfigWrPrepScript]");
                downloadScript(KONST.CONFIG_WR_PREP, DevFile.PartsList[ActivePart].ConfigWrPrepScript);
            }
            // config write       
            if (DevFile.PartsList[ActivePart].ConfigWrScript != 0) // don't download non-existant scripts
            {
                Program.mCmdLogScripts.WriteLine("[#ConfigWrScript]");
                downloadScript(KONST.CONFIG_WR, DevFile.PartsList[ActivePart].ConfigWrScript);
            }
            // prep for user id read      
            if (DevFile.PartsList[ActivePart].UserIDRdPrepScript != 0) // don't download non-existant scripts
            {
                Program.mCmdLogScripts.WriteLine("[#UserIDRdPrepScript]");
                downloadScript(KONST.USERID_RD_PREP, DevFile.PartsList[ActivePart].UserIDRdPrepScript);
            }
            // user id read      
            if (DevFile.PartsList[ActivePart].UserIDRdScript != 0) // don't download non-existant scripts
            {
                Program.mCmdLogScripts.WriteLine("[#UserIDRdScript]");
                downloadScript(KONST.USERID_RD, DevFile.PartsList[ActivePart].UserIDRdScript);
            }
            // prep for user id write      
            if (DevFile.PartsList[ActivePart].UserIDWrPrepScript != 0) // don't download non-existant scripts
            {
                Program.mCmdLogScripts.WriteLine("[#UserIDWrPrepScript]");
                downloadScript(KONST.USERID_WR_PREP, DevFile.PartsList[ActivePart].UserIDWrPrepScript);
            }
            // user id write      
            if (DevFile.PartsList[ActivePart].UserIDWrScript != 0) // don't download non-existant scripts
            {
                Program.mCmdLogScripts.WriteLine("[#UserIDWrScript]");
                downloadScript(KONST.USERID_WR, DevFile.PartsList[ActivePart].UserIDWrScript);
            }
            // read osscal      
            if (DevFile.PartsList[ActivePart].OSCCALRdScript != 0) // don't download non-existant scripts
            {
                Program.mCmdLogScripts.WriteLine("[#OSCCALRdScript]");
                downloadScript(KONST.OSSCAL_RD, DevFile.PartsList[ActivePart].OSCCALRdScript);
            }
            // write osscal      
            if (DevFile.PartsList[ActivePart].OSCCALWrScript != 0) // don't download non-existant scripts
            {
                Program.mCmdLogScripts.WriteLine("[#OSCCALWrScript]");
                downloadScript(KONST.OSSCAL_WR, DevFile.PartsList[ActivePart].OSCCALWrScript);
            }
            // chip erase      
            if (DevFile.PartsList[ActivePart].ChipEraseScript != 0) // don't download non-existant scripts
            {
                Program.mCmdLogScripts.WriteLine("[#ChipEraseScript]");
                downloadScript(KONST.ERASE_CHIP, DevFile.PartsList[ActivePart].ChipEraseScript);
            }
            // program memory erase 
            if (DevFile.PartsList[ActivePart].ProgMemEraseScript != 0) // don't download non-existant scripts
            {
                Program.mCmdLogScripts.WriteLine("[#ProgMemEraseScript]");
                downloadScript(KONST.ERASE_PROGMEM, DevFile.PartsList[ActivePart].ProgMemEraseScript);
            }
            // ee erase 
            if (DevFile.PartsList[ActivePart].EEMemEraseScript != 0) // don't download non-existant scripts
            {
                Program.mCmdLogScripts.WriteLine("[#EEMemEraseScript]");
                downloadScript(KONST.ERASE_EE, DevFile.PartsList[ActivePart].EEMemEraseScript);
            }
            // row erase
            if (DevFile.PartsList[ActivePart].DebugRowEraseScript != 0) // don't download non-existant scripts
            {
                Program.mCmdLogScripts.WriteLine("[#DebugRowEraseScript]");
                downloadScript(KONST.ROW_ERASE, DevFile.PartsList[ActivePart].DebugRowEraseScript);
            }
            // Test Memory Read
            if (DevFile.PartsList[ActivePart].TestMemoryRdScript != 0) // don't download non-existant scripts
            {
                Program.mCmdLogScripts.WriteLine("[#TestMemoryRdScript]");
                downloadScript(KONST.TESTMEM_RD, DevFile.PartsList[ActivePart].TestMemoryRdScript);
            }
            // EE Row Erase
            if (DevFile.PartsList[ActivePart].EERowEraseScript != 0) // don't download non-existant scripts
            {
                Program.mCmdLogScripts.WriteLine("[#EERowEraseScript]");
                downloadScript(KONST.EEROW_ERASE, DevFile.PartsList[ActivePart].EERowEraseScript);
            }

            // get script buffer checksum
            scriptBufferChecksum = getScriptBufferChecksum();
        }

        private static uint getScriptBufferChecksum()
        {
            if (LearnMode)
                return 0;

            byte[] commandArray = new byte[1];
            commandArray[0] = KONST.SCRIPT_BUFFER_CHKSUM;
            if (writeUSB(commandArray))
            {
                if (readUSB())
                {
                    uint checksum = (uint)Usb_read_array[4];
                    checksum += (uint)(Usb_read_array[3] << 8);
                    checksum += (uint)(Usb_read_array[2] << 16);
                    checksum += (uint)(Usb_read_array[1] << 24);

                    return checksum;
                }
                return 0;
            }
            return 0;
        }

        private static bool downloadScript(byte scriptBufferLocation, int scriptArrayIndex)
        {
            // see if we've already downloaded the script.  Some devices use the same script
            // for different functions.  Not downloading it several times saves space in the script buffer
            byte redirectTo = scriptBufferLocation;  // default doesn't redirect; calls itself
            for (byte i = 0; i < scriptRedirectTable.Length; i++)
            {
                if (scriptArrayIndex == scriptRedirectTable[i].deviceFileScriptNumber)
                {
                    redirectTo = i; // redirect to this buffer location
                    break;
                }
            }
            scriptRedirectTable[scriptBufferLocation].redirectToScriptLocation = redirectTo; // set redirection
            scriptRedirectTable[scriptBufferLocation].deviceFileScriptNumber = scriptArrayIndex;
            // note: since the FOR loop above always finds the first instance of a script, we don't have to
            // worry about redirecting to a redirect.
            if (scriptBufferLocation != redirectTo)
            {  // we're redirecting
                return true;  // we're all done
            }

            int scriptLength = DevFile.Scripts[--scriptArrayIndex].ScriptLength;

            byte[] commandArray = new byte[3 + scriptLength];
            commandArray[0] = KONST.DOWNLOAD_SCRIPT;
            commandArray[1] = scriptBufferLocation;
            commandArray[2] = (byte)scriptLength;
            for (int n = 0; n < scriptLength; n++)
            {
                ushort scriptEntry = DevFile.Scripts[scriptArrayIndex].Script[n];
                if (fastProgramming)
                {
                    commandArray[3 + n] = (byte)scriptEntry;
                }
                else
                {
                    if (scriptEntry == 0xAAE7)
                    { // delay short
                        ushort nextEntry = (ushort)(DevFile.Scripts[scriptArrayIndex].Script[n + 1] & 0xFF);
                        if ((nextEntry < 170) && (nextEntry != 0))
                        {
                            commandArray[3 + n++] = (byte)scriptEntry;
                            byte delay = (byte)DevFile.Scripts[scriptArrayIndex].Script[n];
                            commandArray[3 + n] = (byte)(delay + (delay / 2)); //1.5x delay   
                            //commandArray[3 + n] = (byte)(2 * DevFile.Scripts[scriptArrayIndex].Script[n]);
                        }
                        else
                        {
                            commandArray[3 + n++] = KONST._DELAY_LONG;
                            commandArray[3 + n] = 2;
                        }
                    }
                    else if (scriptEntry == 0xAAE8)
                    { // delay long
                        ushort nextEntry = (ushort)(DevFile.Scripts[scriptArrayIndex].Script[n + 1] & 0xFF);
                        if ((nextEntry < 171) && (nextEntry != 0))
                        {
                            commandArray[3 + n++] = (byte)scriptEntry;
                            byte delay = (byte)DevFile.Scripts[scriptArrayIndex].Script[n];
                            commandArray[3 + n] = (byte)(delay + (delay / 2)); //1.5x delay
                        }
                        else
                        {
                            commandArray[3 + n++] = KONST._DELAY_LONG;
                            commandArray[3 + n] = 0; // max out
                        }
                    }
                    else
                    {
                        commandArray[3 + n] = (byte)scriptEntry;
                    }
                }
            }
            return writeUSB(commandArray);
        }


        public static bool writeBT(byte[] commandList)
        {
            byte[] tmpBuffer = new byte[256];
            int length;

            length = BT.processCmdBuff(commandList, commandList.Length, tmpBuffer);

            DateTime d = DateTime.Now;

            TimeSpan diff1, diff2;

            try
            {
                //timijk 2015.12.31
                //spHandle.Write(commandList, 0, commandList.Length);
                spHandle.Write(tmpBuffer, 0, length);

                diff1 = DateTime.Now - d;

                writeBT_latency_count++;

                //Thread.Sleep(25);

                if (writeBT_latency_count == 7) writeBTflushACK();

                //timijk 2016.01.02 wait for the complete signal
                //spHandle.ReadByte();


            }
            catch
            {
                return false;
            }

            diff2 = DateTime.Now - d;

            writeBT_cnt++;
            writeBT_acc_response_time1 += diff1.TotalMilliseconds;
            writeBT_acc_response_time2 += diff2.TotalMilliseconds;

            if (diff2.TotalMilliseconds > writeBT_acc_response_time2Max)
                writeBT_acc_response_time2Max = diff2.TotalMilliseconds;

            double avg1 = writeBT_acc_response_time1 / (double)writeBT_cnt;
            double avg2 = writeBT_acc_response_time2 / (double)writeBT_cnt;
            double avgtck = (double)writeBT_total_tick / (double)writeBT_cnt;

            //use 36 for initial connecting stage
            //use 200 for PIC16F690 Program Memory READ
            //use 512 for PIC18F2550 Program Memory READ
            if (writeBT_cnt == 512)
            {
                avg1 = avg1;
                avg2 = avg2;
                avgtck = avgtck;
            }

            if (diff2.TotalMilliseconds > 100.0)
            {
                avg1 = avg1;
            }
            return true;
        }

        //timijk 2016.01.22 Flush the ACK signal              
        public static void writeBTflushACK()
        {
            for (; writeBT_latency_count > 0; writeBT_latency_count--)
            {
                writeBT_curr_latency_count = spHandle.ReadByte();
                writeBT_total_tick += writeBT_curr_latency_count;
            }

        }

        public static bool readBT()
        {

            if (LearnMode) return true;

            try
            {
                BT.getResponse();

                //timijk 2015.12.31 from 1 to 65
                for (int i = 1; i < Usb_read_array.Length; i++)
                    Usb_read_array[i] = (byte)spHandle.ReadByte();

                //timijk 2016.01.21 .... performance test
                //c = Usb_read_array.Length - 1;

                //while( c>0 )
                //{
                //    c= c - spHandle.Read(Usb_read_array, Usb_read_array.Length-c, c);
                //}
            }
            catch
            {
                return false;
            }

            //?timijk
            if (Usb_read_array[1] == 'E')
            {
                Usb_read_array[1] = Usb_read_array[1];
            }
            return true;


        }

        private static void setupDTBLs(DTBL tblInfo, DTBL tblFamilies, DTBL tblPartsList, DTBL tblScripts)
        {

            tblInfo.Columns.Add("VersionMajor", System.Type.GetType("System.Int32"));
            tblInfo.Columns.Add("VersionMinor", System.Type.GetType("System.Int32"));
            tblInfo.Columns.Add("VersionDot", System.Type.GetType("System.Int32"));
            tblInfo.Columns.Add("VersionNotes", System.Type.GetType("System.String"));
            tblInfo.Columns.Add("NumberFamilies", System.Type.GetType("System.Int32"));
            tblInfo.Columns.Add("NumberParts", System.Type.GetType("System.Int32"));
            tblInfo.Columns.Add("NumberScripts", System.Type.GetType("System.Int32"));
            tblInfo.Columns.Add("Compatibility", System.Type.GetType("System.Byte"));
            tblInfo.Columns.Add("UNUSED1A", System.Type.GetType("System.Byte"));
            tblInfo.Columns.Add("UNUSED1B", System.Type.GetType("System.UInt16"));
            tblInfo.Columns.Add("UNUSED2", System.Type.GetType("System.UInt32"));

            tblFamilies.Columns.Add("FamilyID", System.Type.GetType("System.UInt16"));
            tblFamilies.Columns.Add("FamilyType", System.Type.GetType("System.UInt16"));
            tblFamilies.Columns.Add("SearchPriority", System.Type.GetType("System.UInt16"));
            tblFamilies.Columns.Add("FamilyName", System.Type.GetType("System.String"));
            tblFamilies.Columns.Add("ProgEntryScript", System.Type.GetType("System.UInt16"));
            tblFamilies.Columns.Add("ProgExitScript", System.Type.GetType("System.UInt16"));
            tblFamilies.Columns.Add("ReadDevIDScript", System.Type.GetType("System.UInt16"));
            tblFamilies.Columns.Add("DeviceIDMask", System.Type.GetType("System.UInt32"));
            tblFamilies.Columns.Add("BlankValue", System.Type.GetType("System.UInt32"));
            tblFamilies.Columns.Add("BytesPerLocation", System.Type.GetType("System.Byte"));
            tblFamilies.Columns.Add("AddressIncrement", System.Type.GetType("System.Byte"));
            tblFamilies.Columns.Add("PartDetect", System.Type.GetType("System.Boolean"));
            tblFamilies.Columns.Add("ProgEntryVPPScript", System.Type.GetType("System.UInt16"));
            tblFamilies.Columns.Add("UNUSED1", System.Type.GetType("System.UInt16"));
            tblFamilies.Columns.Add("EEMemBytesPerWord", System.Type.GetType("System.Byte"));
            tblFamilies.Columns.Add("EEMemAddressIncrement", System.Type.GetType("System.Byte"));
            tblFamilies.Columns.Add("UserIDHexBytes", System.Type.GetType("System.Byte"));
            tblFamilies.Columns.Add("UserIDBytes", System.Type.GetType("System.Byte"));
            tblFamilies.Columns.Add("ProgMemHexBytes", System.Type.GetType("System.Byte"));
            tblFamilies.Columns.Add("EEMemHexBytes", System.Type.GetType("System.Byte"));
            tblFamilies.Columns.Add("ProgMemShift", System.Type.GetType("System.Byte"));
            tblFamilies.Columns.Add("TestMemoryStart", System.Type.GetType("System.UInt32"));
            tblFamilies.Columns.Add("TestMemoryLength", System.Type.GetType("System.UInt16"));
            tblFamilies.Columns.Add("Vpp", System.Type.GetType("System.Single"));

            tblPartsList.Columns.Add("PartName", System.Type.GetType("System.String"));
            tblPartsList.Columns.Add("Family", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("DeviceID", System.Type.GetType("System.UInt32"));
            tblPartsList.Columns.Add("ProgramMem", System.Type.GetType("System.UInt32"));
            tblPartsList.Columns.Add("EEMem", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("EEAddr", System.Type.GetType("System.UInt32"));
            tblPartsList.Columns.Add("ConfigWords", System.Type.GetType("System.Byte"));
            tblPartsList.Columns.Add("ConfigAddr", System.Type.GetType("System.UInt32"));
            tblPartsList.Columns.Add("UserIDWords", System.Type.GetType("System.Byte"));
            tblPartsList.Columns.Add("UserIDAddr", System.Type.GetType("System.UInt32"));
            tblPartsList.Columns.Add("BandGapMask", System.Type.GetType("System.UInt32"));

            tblPartsList.Columns.Add("ConfigMasks", System.Type.GetType("System.UInt16[]"));
            tblPartsList.Columns.Add("ConfigBlank", System.Type.GetType("System.UInt16[]"));

            // Init config arrays
            //DevFile.PartsList[l_x].ConfigMasks = new ushort[KONST.NumConfigMasks];
            //DevFile.PartsList[l_x].ConfigBlank = new ushort[KONST.NumConfigMasks];
            //for (int l_index = 0; l_index < KONST.MaxReadCfgMasks; l_index++)
            //{
            //    DevFile.PartsList[l_x].ConfigMasks[l_index] = binRead.ReadUInt16();
            //}
            //for (int l_index = 0; l_index < KONST.MaxReadCfgMasks; l_index++)
            //{
            //    DevFile.PartsList[l_x].ConfigBlank[l_index] = binRead.ReadUInt16();
            //}

            tblPartsList.Columns.Add("CPMask", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("CPConfig", System.Type.GetType("System.Byte"));
            tblPartsList.Columns.Add("OSSCALSave", System.Type.GetType("System.Boolean"));
            tblPartsList.Columns.Add("IgnoreAddress", System.Type.GetType("System.UInt32"));
            tblPartsList.Columns.Add("VddMin", System.Type.GetType("System.Single"));
            tblPartsList.Columns.Add("VddMax", System.Type.GetType("System.Single"));
            tblPartsList.Columns.Add("VddErase", System.Type.GetType("System.Single"));
            tblPartsList.Columns.Add("CalibrationWords", System.Type.GetType("System.Byte"));
            tblPartsList.Columns.Add("ChipEraseScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("ProgMemAddrSetScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("ProgMemAddrBytes", System.Type.GetType("System.Byte"));
            tblPartsList.Columns.Add("ProgMemRdScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("ProgMemRdWords", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("EERdPrepScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("EERdScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("EERdLocations", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("UserIDRdPrepScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("UserIDRdScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("ConfigRdPrepScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("ConfigRdScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("ProgMemWrPrepScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("ProgMemWrScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("ProgMemWrWords", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("ProgMemPanelBufs", System.Type.GetType("System.Byte"));
            tblPartsList.Columns.Add("ProgMemPanelOffset", System.Type.GetType("System.UInt32"));
            tblPartsList.Columns.Add("EEWrPrepScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("EEWrScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("EEWrLocations", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("UserIDWrPrepScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("UserIDWrScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("ConfigWrPrepScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("ConfigWrScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("OSCCALRdScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("OSCCALWrScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("DPMask", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("WriteCfgOnErase", System.Type.GetType("System.Boolean"));
            tblPartsList.Columns.Add("BlankCheckSkipUsrIDs", System.Type.GetType("System.Boolean"));
            tblPartsList.Columns.Add("IgnoreBytes", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("ChipErasePrepScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("BootFlash", System.Type.GetType("System.UInt32"));
            tblPartsList.Columns.Add("Config9Mask", System.Type.GetType("System.UInt16"));
            //tblPartsList.Columns.Add("ConfigMasks[8] = tblPartsList.Columns.Add("Config9Mask;
            tblPartsList.Columns.Add("Config9Blank", System.Type.GetType("System.UInt16"));
            //tblPartsList.Columns.Add("ConfigBlank[8] = tblPartsList.Columns.Add("Config9Blank;
            tblPartsList.Columns.Add("ProgMemEraseScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("EEMemEraseScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("ConfigMemEraseScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("reserved1EraseScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("reserved2EraseScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("TestMemoryRdScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("TestMemoryRdWords", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("EERowEraseScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("EERowEraseWords", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("ExportToMPLAB", System.Type.GetType("System.Boolean"));
            tblPartsList.Columns.Add("DebugHaltScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("DebugRunScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("DebugStatusScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("DebugReadExecVerScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("DebugSingleStepScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("DebugBulkWrDataScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("DebugBulkRdDataScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("DebugWriteVectorScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("DebugReadVectorScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("DebugRowEraseScript", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("DebugRowEraseSize", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("DebugReserved5Script", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("DebugReserved6Script", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("DebugReserved7Script", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("DebugReserved8Script", System.Type.GetType("System.UInt16"));
            tblPartsList.Columns.Add("LVPScript", System.Type.GetType("System.UInt16"));



            tblScripts.Columns.Add("ScriptNumber", System.Type.GetType("System.UInt16"));
            tblScripts.Columns.Add("ScriptName", System.Type.GetType("System.String"));
            tblScripts.Columns.Add("ScriptVersion", System.Type.GetType("System.UInt16"));
            tblScripts.Columns.Add("UNUSED1", System.Type.GetType("System.UInt32"));
            tblScripts.Columns.Add("ScriptLength", System.Type.GetType("System.UInt16"));
            tblScripts.Columns.Add("Script", System.Type.GetType("System.UInt16[]"));

            // init script array
            // DevFile.Scripts[l_x].Script = new ushort[DevFile.Scripts[l_x].ScriptLength];
            //for (int l_index = 0; l_index < DevFile.Scripts[l_x].ScriptLength; l_index++)
            //{
            //   DevFile.Scripts[l_x].Script[l_index] = binRead.ReadUInt16();
            // }

            tblScripts.Columns.Add("Comment", System.Type.GetType("System.String"));
        }

        private static void addTblInfo(DTBL tblInfo, DeviceFile.DeviceFileParams fInfo)
        {
            System.Data.DataRow myNewRow;

            myNewRow = tblInfo.NewRow();

            myNewRow["VersionMajor"] = fInfo.VersionMajor; //Int32();
            myNewRow["VersionMinor"] = fInfo.VersionMinor; //Int32();
            myNewRow["VersionDot"] = fInfo.VersionDot; //Int32();
            myNewRow["VersionNotes"] = fInfo.VersionNotes; //String();
            myNewRow["NumberFamilies"] = fInfo.NumberFamilies; //Int32();
            myNewRow["NumberParts"] = fInfo.NumberParts; //Int32();
            myNewRow["NumberScripts"] = fInfo.NumberScripts; //Int32();
            myNewRow["Compatibility"] = fInfo.Compatibility; //Byte();
            myNewRow["UNUSED1A"] = fInfo.UNUSED1A; //Byte();
            myNewRow["UNUSED1B"] = fInfo.UNUSED1B; //UInt16();
            myNewRow["UNUSED2"] = fInfo.UNUSED2; //UInt32();

            tblInfo.Rows.Add(myNewRow);
        }

        private static void addTblFamilies(DTBL tblFamilies, DeviceFile.DeviceFamilyParams[] fFamilies)
        {
            System.Data.DataRow myNewRow;

            for (int i = 0; i < fFamilies.Length; i++)
            {
                myNewRow = tblFamilies.NewRow();

                myNewRow["FamilyID"] = fFamilies[i].FamilyID; //UInt16();
                myNewRow["FamilyType"] = fFamilies[i].FamilyType; //UInt16();
                myNewRow["SearchPriority"] = fFamilies[i].SearchPriority; //UInt16();
                myNewRow["FamilyName"] = fFamilies[i].FamilyName; //String();
                myNewRow["ProgEntryScript"] = fFamilies[i].ProgEntryScript; //UInt16();
                myNewRow["ProgExitScript"] = fFamilies[i].ProgExitScript; //UInt16();
                myNewRow["ReadDevIDScript"] = fFamilies[i].ReadDevIDScript; //UInt16();
                myNewRow["DeviceIDMask"] = fFamilies[i].DeviceIDMask; //UInt32();
                myNewRow["BlankValue"] = fFamilies[i].BlankValue; //UInt32();
                myNewRow["BytesPerLocation"] = fFamilies[i].BytesPerLocation; //Byte();
                myNewRow["AddressIncrement"] = fFamilies[i].AddressIncrement; //Byte();
                myNewRow["PartDetect"] = fFamilies[i].PartDetect; //Boolean();
                myNewRow["ProgEntryVPPScript"] = fFamilies[i].ProgEntryVPPScript; //UInt16();
                myNewRow["UNUSED1"] = fFamilies[i].UNUSED1; //UInt16();
                myNewRow["EEMemBytesPerWord"] = fFamilies[i].EEMemBytesPerWord; //Byte();
                myNewRow["EEMemAddressIncrement"] = fFamilies[i].EEMemAddressIncrement; //Byte();
                myNewRow["UserIDHexBytes"] = fFamilies[i].UserIDHexBytes; //Byte();
                myNewRow["UserIDBytes"] = fFamilies[i].UserIDBytes; //Byte();
                myNewRow["ProgMemHexBytes"] = fFamilies[i].ProgMemHexBytes; //Byte();
                myNewRow["EEMemHexBytes"] = fFamilies[i].EEMemHexBytes; //Byte();
                myNewRow["ProgMemShift"] = fFamilies[i].ProgMemShift; //Byte();
                myNewRow["TestMemoryStart"] = fFamilies[i].TestMemoryStart; //UInt32();
                myNewRow["TestMemoryLength"] = fFamilies[i].TestMemoryLength; //UInt16();
                myNewRow["Vpp"] = fFamilies[i].Vpp; //Single();

                tblFamilies.Rows.Add(myNewRow);
            }

        }

        private static void addTblPartsList(DTBL tblPartsList, DeviceFile.DevicePartParams[] fPartsList)
        {

            System.Data.DataRow myNewRow;

            for (int i = 0; i < fPartsList.Length; i++)
            {
                myNewRow = tblPartsList.NewRow();

                myNewRow["PartName"] = fPartsList[i].PartName; //String();
                myNewRow["Family"] = fPartsList[i].Family; //UInt16();
                myNewRow["DeviceID"] = fPartsList[i].DeviceID; //UInt32();
                myNewRow["ProgramMem"] = fPartsList[i].ProgramMem; //UInt32();
                myNewRow["EEMem"] = fPartsList[i].EEMem; //UInt16();
                myNewRow["EEAddr"] = fPartsList[i].EEAddr; //UInt32();
                myNewRow["ConfigWords"] = fPartsList[i].ConfigWords; //Byte();
                myNewRow["ConfigAddr"] = fPartsList[i].ConfigAddr; //UInt32();
                myNewRow["UserIDWords"] = fPartsList[i].UserIDWords; //Byte();
                myNewRow["UserIDAddr"] = fPartsList[i].UserIDAddr; //UInt32();
                myNewRow["BandGapMask"] = fPartsList[i].BandGapMask; //UInt32();
                // Init config arrays
                myNewRow["ConfigMasks"] = fPartsList[i].ConfigMasks;
                myNewRow["ConfigBlank"] = fPartsList[i].ConfigBlank;
                myNewRow["CPMask"] = fPartsList[i].CPMask; //UInt16();
                myNewRow["CPConfig"] = fPartsList[i].CPConfig; //Byte();
                myNewRow["OSSCALSave"] = fPartsList[i].OSSCALSave; //Boolean();
                myNewRow["IgnoreAddress"] = fPartsList[i].IgnoreAddress; //UInt32();
                myNewRow["VddMin"] = fPartsList[i].VddMin; //Single();
                myNewRow["VddMax"] = fPartsList[i].VddMax; //Single();
                myNewRow["VddErase"] = fPartsList[i].VddErase; //Single();
                myNewRow["CalibrationWords"] = fPartsList[i].CalibrationWords; //Byte();
                myNewRow["ChipEraseScript"] = fPartsList[i].ChipEraseScript; //UInt16();
                myNewRow["ProgMemAddrSetScript"] = fPartsList[i].ProgMemAddrSetScript; //UInt16();
                myNewRow["ProgMemAddrBytes"] = fPartsList[i].ProgMemAddrBytes; //Byte();
                myNewRow["ProgMemRdScript"] = fPartsList[i].ProgMemRdScript; //UInt16();
                myNewRow["ProgMemRdWords"] = fPartsList[i].ProgMemRdWords; //UInt16();
                myNewRow["EERdPrepScript"] = fPartsList[i].EERdPrepScript; //UInt16();
                myNewRow["EERdScript"] = fPartsList[i].EERdScript; //UInt16();
                myNewRow["EERdLocations"] = fPartsList[i].EERdLocations; //UInt16();
                myNewRow["UserIDRdPrepScript"] = fPartsList[i].UserIDRdPrepScript; //UInt16();
                myNewRow["UserIDRdScript"] = fPartsList[i].UserIDRdScript; //UInt16();
                myNewRow["ConfigRdPrepScript"] = fPartsList[i].ConfigRdPrepScript; //UInt16();
                myNewRow["ConfigRdScript"] = fPartsList[i].ConfigRdScript; //UInt16();
                myNewRow["ProgMemWrPrepScript"] = fPartsList[i].ProgMemWrPrepScript; //UInt16();
                myNewRow["ProgMemWrScript"] = fPartsList[i].ProgMemWrScript; //UInt16();
                myNewRow["ProgMemWrWords"] = fPartsList[i].ProgMemWrWords; //UInt16();
                myNewRow["ProgMemPanelBufs"] = fPartsList[i].ProgMemPanelBufs; //Byte();
                myNewRow["ProgMemPanelOffset"] = fPartsList[i].ProgMemPanelOffset; //UInt32();
                myNewRow["EEWrPrepScript"] = fPartsList[i].EEWrPrepScript; //UInt16();
                myNewRow["EEWrScript"] = fPartsList[i].EEWrScript; //UInt16();
                myNewRow["EEWrLocations"] = fPartsList[i].EEWrLocations; //UInt16();
                myNewRow["UserIDWrPrepScript"] = fPartsList[i].UserIDWrPrepScript; //UInt16();
                myNewRow["UserIDWrScript"] = fPartsList[i].UserIDWrScript; //UInt16();
                myNewRow["ConfigWrPrepScript"] = fPartsList[i].ConfigWrPrepScript; //UInt16();
                myNewRow["ConfigWrScript"] = fPartsList[i].ConfigWrScript; //UInt16();
                myNewRow["OSCCALRdScript"] = fPartsList[i].OSCCALRdScript; //UInt16();
                myNewRow["OSCCALWrScript"] = fPartsList[i].OSCCALWrScript; //UInt16();
                myNewRow["DPMask"] = fPartsList[i].DPMask; //UInt16();
                myNewRow["WriteCfgOnErase"] = fPartsList[i].WriteCfgOnErase; //Boolean();
                myNewRow["BlankCheckSkipUsrIDs"] = fPartsList[i].BlankCheckSkipUsrIDs; //Boolean();
                myNewRow["IgnoreBytes"] = fPartsList[i].IgnoreBytes; //UInt16();
                myNewRow["ChipErasePrepScript"] = fPartsList[i].ChipErasePrepScript; //UInt16();
                myNewRow["BootFlash"] = fPartsList[i].BootFlash; //UInt32();
                myNewRow["Config9Mask"] = fPartsList[i].Config9Mask; //UInt16();
                myNewRow["Config9Blank"] = fPartsList[i].Config9Blank; //UInt16();
                myNewRow["ProgMemEraseScript"] = fPartsList[i].ProgMemEraseScript; //UInt16();
                myNewRow["EEMemEraseScript"] = fPartsList[i].EEMemEraseScript; //UInt16();
                myNewRow["ConfigMemEraseScript"] = fPartsList[i].ConfigMemEraseScript; //UInt16();
                myNewRow["reserved1EraseScript"] = fPartsList[i].reserved1EraseScript; //UInt16();
                myNewRow["reserved2EraseScript"] = fPartsList[i].reserved2EraseScript; //UInt16();
                myNewRow["TestMemoryRdScript"] = fPartsList[i].TestMemoryRdScript; //UInt16();
                myNewRow["TestMemoryRdWords"] = fPartsList[i].TestMemoryRdWords; //UInt16();
                myNewRow["EERowEraseScript"] = fPartsList[i].EERowEraseScript; //UInt16();
                myNewRow["EERowEraseWords"] = fPartsList[i].EERowEraseWords; //UInt16();
                myNewRow["ExportToMPLAB"] = fPartsList[i].ExportToMPLAB; //Boolean();
                myNewRow["DebugHaltScript"] = fPartsList[i].DebugHaltScript; //UInt16();
                myNewRow["DebugRunScript"] = fPartsList[i].DebugRunScript; //UInt16();
                myNewRow["DebugStatusScript"] = fPartsList[i].DebugStatusScript; //UInt16();
                myNewRow["DebugReadExecVerScript"] = fPartsList[i].DebugReadExecVerScript; //UInt16();
                myNewRow["DebugSingleStepScript"] = fPartsList[i].DebugSingleStepScript; //UInt16();
                myNewRow["DebugBulkWrDataScript"] = fPartsList[i].DebugBulkWrDataScript; //UInt16();
                myNewRow["DebugBulkRdDataScript"] = fPartsList[i].DebugBulkRdDataScript; //UInt16();
                myNewRow["DebugWriteVectorScript"] = fPartsList[i].DebugWriteVectorScript; //UInt16();
                myNewRow["DebugReadVectorScript"] = fPartsList[i].DebugReadVectorScript; //UInt16();
                myNewRow["DebugRowEraseScript"] = fPartsList[i].DebugRowEraseScript; //UInt16();
                myNewRow["DebugRowEraseSize"] = fPartsList[i].DebugRowEraseSize; //UInt16();
                myNewRow["DebugReserved5Script"] = fPartsList[i].DebugReserved5Script; //UInt16();
                myNewRow["DebugReserved6Script"] = fPartsList[i].DebugReserved6Script; //UInt16();
                myNewRow["DebugReserved7Script"] = fPartsList[i].DebugReserved7Script; //UInt16();
                myNewRow["DebugReserved8Script"] = fPartsList[i].DebugReserved8Script; //UInt16();
                myNewRow["LVPScript"] = fPartsList[i].LVPScript; //UInt16();

                tblPartsList.Rows.Add(myNewRow);

            }
        }

        private static void addTblScripts(DTBL tblScripts, DeviceFile.DeviceScripts[] fScripts)
        {
            System.Data.DataRow myNewRow;

            for (int i = 0; i < fScripts.Length; i++)
            {
                myNewRow = tblScripts.NewRow();

                myNewRow["ScriptNumber"] = fScripts[i].ScriptNumber; //UInt16();
                myNewRow["ScriptName"] = fScripts[i].ScriptName; //String();
                myNewRow["ScriptVersion"] = fScripts[i].ScriptVersion; //UInt16();
                myNewRow["UNUSED1"] = fScripts[i].UNUSED1; //UInt32();
                myNewRow["ScriptLength"] = fScripts[i].ScriptLength; //UInt16();
                myNewRow["Script"] = fScripts[i].Script;  //Unit16[];
                myNewRow["Comment"] = fScripts[i].Comment; //String();

                tblScripts.Rows.Add(myNewRow);
            }
        }

        private static void addPIC32Parts(DTBL tblPartsList)
        {
            //System.Data.DataRow myNewRow;

            System.Data.DataRow pic32Row = tblPartsList.Rows[0];

            foreach (PIC32Device.dev dev in PIC32Device.devices)
            {
                pic32Row["PartName"] = dev.PartName;
                pic32Row["DeviceID"] = dev.DeviceID;
                pic32Row["BootFlash"] = dev.BootFlash;
                pic32Row["ConfigAddr"] = dev.ConfigAddr;
                pic32Row["ConfigBlank"] = dev.ConfigBlank;
                pic32Row["ConfigMasks"] = dev.ConfigMasks;
                pic32Row["ConfigWords"] = dev.ConfigWords;
                pic32Row["CPConfig"] = dev.CPConfig;
                pic32Row["ProgramMem"] = dev.ProgramMem;
                pic32Row["UserIDAddr"] = dev.UserIDAddr;

                tblPartsList.ImportRow(pic32Row);

                //tblPartsList.Rows.Add(myNewRow);
            }


        }

    }
}

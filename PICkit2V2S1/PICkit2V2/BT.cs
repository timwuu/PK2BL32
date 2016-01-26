using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using KONST = PICkit2V2.Constants;
using KFN = PICkit2V2.PICkitFunctions;

namespace PICkit2V2
{
    public class BT
    {
        // terminal testing
        //const Byte STX = (Byte)'S'; //0x55;
        //const Byte ETX = (Byte)'E'; //0x04;
        //const Byte DLE = (Byte)'D'; //0x05;

        const Byte STX = 0x55;
        const Byte ETX = 0x04;
        const Byte DLE = 0x05;

        private static SerialPort sp;

        public static bool Find_This_Device(ushort p_VendorID,
                                   ushort p_PoductID,
                                   ushort p_index,
                                   ref SerialPort p_SerialPort)
        {
            int cnt = 0;
            p_SerialPort = null;

            try
            {
                byte[] cmd = { 0x76 }; //0x5A, 0x76, 0xAD };   //NOP,FW_VER,EOB
                byte[] tmpBuffer = new byte[256];
                int length;

                if (sp == null) sp = new SerialPort("COM4");
                sp.BaudRate = 115200;  //timijk 2016.01.22
                cnt++;
                if (!sp.IsOpen)
                {
                    cnt++;
                    sp.Open();
                    cnt++;
                    sp.ReadTimeout = SerialPort.InfiniteTimeout;
                    //sp.ReceivedBytesThreshold = (int)KONST.PACKET_SIZE;
                }

                cnt++;
                //timijk 2015.12.31 sp.Write(cmd, 0, 3);
                length = BT.processCmdBuff(cmd, cmd.Length, tmpBuffer);
                try
                {
                    sp.Write(tmpBuffer, 0, length);
                    //timijk 2016.01.02 wait for the complete signal
                    sp.ReadByte();
                }
                catch
                {
                    return false;
                }

                cnt++;

                //timijk 2015.12.31 getResponse
                getResponse();

                cnt++;
                for (int i = 1; i < KFN.Usb_read_array.Length; i++)  // from 1 to 65
                { KFN.Usb_read_array[i] = (byte)sp.ReadByte(); }

                if (KFN.Usb_read_array[1] == KONST.FWVerMajorReq)
                {
                    if (((KFN.Usb_read_array[2] == KONST.FWVerMinorReq) &&
                        (KFN.Usb_read_array[3] >= KONST.FWVerDotReq)) ||
                        (KFN.Usb_read_array[2] > KONST.FWVerMinorReq))
                    {
                        cnt++;
                        p_SerialPort = sp;
                        USB.UnitID = "BT@" + sp.PortName;
                        return true;
                    }
                }

                if (KFN.Usb_read_array[1] == 'v')
                {
                    KFN.FirmwareVersion = string.Format("BL {0:D1}.{1:D1}", KFN.Usb_read_array[7], KFN.Usb_read_array[8]);
                    p_SerialPort = sp;
                    USB.UnitID = "BT@" + sp.PortName;
                    return true;
                }

            }
            catch (Exception e)
            {
                // timijk 2015.04.08
                System.Windows.Forms.MessageBox.Show(e.ToString() + " after step " + cnt.ToString(), "Find_This_Device:Exception");
                if (sp.IsOpen) sp.Close();
                return false;
            }

            if (sp.IsOpen) sp.Close();
            return false;
        }

        public static void getResponse()
        {
            //timijk 2016.01.22 Flush the ACK signal              
            KFN.writeBTflushACK();

            byte[] cmdGetResponse = { STX, STX, 0x00, ETX };
            //timijk 2015.12.31 getResponse
            //Thread.Sleep(1);  //timijk 2015.12.31 sleep 1ms
            sp.Write(cmdGetResponse, 0, cmdGetResponse.Length);
        }

        public static int processCmdBuff(byte[] cmdBuff, int length, byte[] wrBuff)
        {
            int i, j = 0;

            Byte checkSum = 0;

            // two start signal <STX>
            wrBuff[j++] = STX;
            wrBuff[j++] = STX;

            for (i = 0; i < length; i++, j++)
            {
                switch (cmdBuff[i])
                {
                    case STX:
                    case ETX:
                    case DLE:
                        wrBuff[j++] = DLE;
                        break;

                    default:
                        break;

                }

                checkSum += cmdBuff[i];
                wrBuff[j] = cmdBuff[i];
            }

            //<checksum><ETX>
            Byte cksum = (Byte)((~checkSum + 1) & 0xFF);

            switch (cksum)
            {
                case STX:
                case ETX:
                case DLE:
                    wrBuff[j++] = DLE;
                    break;

                default:
                    break;
            }

            wrBuff[j++] = cksum;
            wrBuff[j++] = ETX;

            return j;
        }

    }
}

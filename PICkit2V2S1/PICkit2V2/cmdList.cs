using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.IO;

namespace PICkit2V2
{
    public class cmdList
    {
        public class cmd
        {
            public string name;
            public byte id;
            public int narg;
            public bool plusN;

            public cmd(string _name, int _narg, bool _plusN)
            {
                name = _name;
                narg = _narg;
                plusN = _plusN;
            }
        }

        static cmd[] cmds = new cmd[] { 
            new cmd("SETVDD",3,false), //0xA0
            new cmd("SETVPP",3,false),//0xA1
            new cmd("READ_STATUS",0,false),//0xA2
            new cmd("READ_VOLTAGES",0,false),//0xA3
            new cmd("DOWNLOAD_SCRIPT",2,true),//0xA4
            new cmd("RUN_SCRIPT",2,false),//0xA5
            new cmd("EXECUTE_SCRIPT",1,true),//0xA6
            new cmd("CLR_DOWNLOAD_BUFFER",0,false),//0xA7
            new cmd("DOWNLOAD_DATA",1,true),//0xA8
            new cmd("CLR_UPLOAD_BUFFER",0,false),//0xA9
            new cmd("UPLOAD_DATA",0,false),//0xAA
            new cmd("CLR_SCRIPT_BUFFER",0,false),//0xAB
            new cmd("UPLOAD_DATA_NOLEN",0,false),//0xAC
            new cmd("END_OF_BUFFER",0,false),//0xAD
            new cmd("RESET",0,false),//0xAE
            new cmd("SCRIPT_BUFFER_CHKSM",0,false),//0xAF
            new cmd("SET_VOLTAGE_CALS",4,false),//0xB0
            new cmd("WR_INTERNAL_EE",2,true),//0xB1
            new cmd("RD_INTERNAL_EE",2,false),//0xB2 ; document error
            new cmd("ENTER_UART_MODE",2,false),//0xB3
            new cmd("EXIT_UART_MODE",0,false),//0xB4
            new cmd("ENTER_LEARN_MODE",4,false),//0xB5
            new cmd("EXIT_LEARN_MODE",0,false),//0xB6
            new cmd("ENABLE_PK2GO_MODE",2,false),//0xB7
            new cmd("LOGIC_ANALYZER_GO",7,false),//0xB8
            new cmd("COPY_RAM_UPLOAD",2,false) //0xB9 
        };

        //META-COMMANDS
        static cmd[] metaCmds ={
            new cmd("READ_OSCCAL",2,false),//0x80
            new cmd("WRITE_OSCCAL",2,false),//0x81
            new cmd("START_CHECKSUM",2,false),//0x82
            new cmd("VERIFY_CHECKSUM",2,false),//0x83
            new cmd("CHECK_DEVICE_ID",4,false),//0x84
            new cmd("READ_BANDGAP",0,false),//0x85
            new cmd("WRITE_CFG_BANDGAP",0,false),//0x86
            new cmd("CHANGE_CHKSM_FRMT",2,false) //0x87
                       };

        public static void processCmdList(TextWriter tw, byte[] commandList)
        {
            tw.WriteLine("00({0}):", commandList.Length.ToString("X2"));

            for (int ind = 0; ind < commandList.Length; ind++)
            {
                ind += getCmdString(tw, commandList, ind);
            }

            tw.WriteLine();

        }


        static int getCmdString(TextWriter tw, byte[] cmdlist, int ind)
        {
            byte cmdID = cmdlist[ind];
            byte id1;
            byte n = 0;
            cmd cmd1;
            bool hasScriptCmd = false;

            if (cmdID < 0x80)
            {
                if (cmdID == 0x42) { tw.WriteLine("ENTER_BOOTLOADER"); return 0; }
                else if (cmdID == 0x5A) { tw.WriteLine("NO_OPERATION"); return 0; }
                else if (cmdID == 0x76) { tw.WriteLine("FIRMWARE_VERSION"); return 0; }
            }
            else if (cmdID < 0x88)
            {
                id1 = (byte)(cmdID - 0x88);
                cmd1 = metaCmds[id1];
                tw.Write(cmd1.name);
                if (cmd1.plusN)
                {

                }
                else
                {
                    for (int i = 0; i < cmd1.narg; i++)
                    {
                        tw.Write(' ');
                        tw.Write(cmdlist[ind + i + 1].ToString("X2"));
                    }
                    tw.WriteLine();
                    return cmd1.narg;
                }
            }
            else if (cmdID < 0xA0)
            {
                tw.WriteLine("ERR");
                return 0;
            }
            else if (cmdID < 0xB9)
            {
                hasScriptCmd = ((cmdID == 0xA4) || (cmdID == 0xA5) || (cmdID == 0xA6));
                id1 = (byte)(cmdID - 0xA0);
                cmd1 = cmds[id1];
                tw.Write(cmd1.name);
                double volt;

                if (cmdID == 0xA0) //SETADD
                {
                    volt = (((double)cmdlist[ind + 1] + (double)cmdlist[ind + 2] * 256.0)/64.0-10.5)/32.0;
                    tw.Write("("+volt.ToString("0.00")+")");
                }
                else if (cmdID == 0xA1) //SETAPP
                {
                    volt = ((double)cmdlist[ind + 2]) /18.61;
                    tw.Write("(" + volt.ToString("0.00") + ")");
                }
                n = 0;
                int i;
                for (i = 0; i < cmd1.narg; i++)
                {
                    n = cmdlist[ind + i + 1];
                    tw.Write(' ');
                    tw.Write(n.ToString("X2"));
                }

                if (!cmd1.plusN) n = 0;

                for (; i < cmd1.narg + n; i++)
                {
                    if (hasScriptCmd)
                    {
                        i += scriptCmdList.getCmdString(tw, cmdlist, (ind + i + 1));
                    }
                    else
                    {
                        tw.Write(' ');
                        tw.Write(cmdlist[ind + i + 1].ToString("X2"));
                    }
                }

                tw.WriteLine();
                return cmd1.narg + n;
            }

            tw.WriteLine("ERR");
            return 0;

        }

    }
}

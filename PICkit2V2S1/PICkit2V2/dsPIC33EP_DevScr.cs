using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pk2 = PICkit2V2.PICkitFunctions;
using KONST = PICkit2V2.Constants;
using DTBL = System.Data.DataTable;

namespace PICkit2V2
{
    class dsPIC33EP_DevScr
    {
        public static DeviceFile.DeviceScripts ChipEraseScript;  // Erase User Memory Only (No PE, UserID)
        public static DeviceFile.DeviceScripts ProgMemRdScript;
        public static DeviceFile.DeviceScripts ProgMemAddrSetScript;
        public static DeviceFile.DeviceScripts ConfigRdScript;
        public static DeviceFile.DeviceScripts UserIDRdScript;

        public static DeviceFile.DeviceScripts ProgMemWrPrepScript;
        public static DeviceFile.DeviceScripts ProgMemWrScript;
        public static DeviceFile.DeviceScripts ProgMemWrAuxScript;

        public static DeviceFile.DeviceScripts ConfigWrScript;
        public static DeviceFile.DeviceScripts DebugWriteVectorScript;  //Erase Programming Executive Memory

        public static void init()
        {
            initChipEraseScript();

            initProgMemAddrSetScript();
            initProgMemRdScript();

            initConfigRdScript();
            initUserIDRdScript();

            initProgMemWrPrepScript();
            initProgMemWrScript();
            initProgMemWrAuxScript();

            initConfigWrScript();
            initDebugWriteVectorScript();
        }

        public static void exportXML()
        {
            DTBL scriptTbl = new DTBL("Scripts");
            DeviceFile.DeviceScripts[] fScripts = new DeviceFile.DeviceScripts[9];

            fScripts[0] = ChipEraseScript;  // Erase User Memory Only (No PE, UserID)
            fScripts[1] = ProgMemRdScript;
            fScripts[2] = ProgMemAddrSetScript;
            fScripts[3] = ConfigRdScript;
            fScripts[4] = UserIDRdScript;
            fScripts[5] = ProgMemWrPrepScript;
            fScripts[6] = ProgMemWrScript;
            //fScripts[7] = ProgMemWrAuxScript;
            fScripts[7] = ConfigWrScript;
            fScripts[8] = DebugWriteVectorScript;  //Erase Programming Executive Memory

            //timijk 2015.06.10
            xmlUtilility.exportTblScripts(fScripts, "d:\\temp\\scriptTbl.xml");
        }

        public static void ProgMemWrite()
        {

            // Set address
            if (Pk2.DevFile.PartsList[Pk2.ActivePart].ProgMemWrPrepScript != 0)
            { // if prog mem address set script exists for this part
                Pk2.DownloadAddress3(0x000000); // start of prog memory
                Pk2.RunScript(KONST.PROGMEM_WR_PREP, 1);
            }

            int commOffSet = 0;
            byte[] commandArrayp = new byte[64];

            //Download Instructions
            commOffSet = 0;
            commandArrayp[commOffSet++] = KONST.CLR_DOWNLOAD_BUFFER;
            commandArrayp[commOffSet++] = KONST.DOWNLOAD_DATA;
            commandArrayp[commOffSet++] = 24;
            commandArrayp[commOffSet++] = 0xf2;   // GOTO 0x200
            commandArrayp[commOffSet++] = 0x02;
            commandArrayp[commOffSet++] = 0x04;
            commandArrayp[commOffSet++] = 0x01;
            commandArrayp[commOffSet++] = 0x02;
            commandArrayp[commOffSet++] = 0x03;
            commandArrayp[commOffSet++] = 0x0F;
            commandArrayp[commOffSet++] = 0x00;
            commandArrayp[commOffSet++] = 0x21;
            commandArrayp[commOffSet++] = 0x0E;
            commandArrayp[commOffSet++] = 0xFF;
            commandArrayp[commOffSet++] = 0x28;
            commandArrayp[commOffSet++] = 0x3F;
            commandArrayp[commOffSet++] = 0x44;
            commandArrayp[commOffSet++] = 0x55;
            commandArrayp[commOffSet++] = 0x66;
            commandArrayp[commOffSet++] = 0xFF;
            commandArrayp[commOffSet++] = 0x77;
            commandArrayp[commOffSet++] = 0x0F;
            commandArrayp[commOffSet++] = 0x99;
            commandArrayp[commOffSet++] = 0x21;
            commandArrayp[commOffSet++] = 0xAA;
            commandArrayp[commOffSet++] = 0xBB;
            commandArrayp[commOffSet++] = 0x38;

            commandArrayp[2] = (byte)(commOffSet - 3);  // script length

            for (; commOffSet < 64; commOffSet++)
            {
                commandArrayp[commOffSet] = KONST.END_OF_BUFFER;
            }
            Pk2.writeUSB(commandArrayp);

            //---- Write Instructions Part 2. Start Writing

            Pk2.RunScript(KONST.PROGMEM_WR, 4);

            //---------------------------------------

            Pk2.RunScript(KONST.PROG_EXIT, 1);

            return;
        }

        public static void initProgMemWrScript()
        {
            ushort i = 0;
            ProgMemWrScript.ScriptNumber = 286;
            ProgMemWrScript.ScriptName = "Prog Memory Write";
            ProgMemWrScript.Comment = "..";
            ProgMemWrScript.ScriptVersion = 0;
            ProgMemWrScript.UNUSED1 = 0;
            ProgMemWrScript.Script = new ushort[60];

            ProgMemWrScript.Script[i++] = KONST._COREINST24;  //CLR W7
            ProgMemWrScript.Script[i++] = 0x80;
            ProgMemWrScript.Script[i++] = 0x03;
            ProgMemWrScript.Script[i++] = 0xEB;
            ProgMemWrScript.Script[i++] = KONST._WRITE_BUFWORD_W;  //MOV BUF, W0  //<--------LOOP{}
            ProgMemWrScript.Script[i++] = 0x00;
            ProgMemWrScript.Script[i++] = KONST._WRITE_BUFBYTE_W;  //MOV.B BUF, W1
            ProgMemWrScript.Script[i++] = 0x01;
            ProgMemWrScript.Script[i++] = KONST._COREINST24;  //TBLWTL W0,[W7]
            ProgMemWrScript.Script[i++] = 0x80;
            ProgMemWrScript.Script[i++] = 0x0B;
            ProgMemWrScript.Script[i++] = 0xBB;
            ProgMemWrScript.Script[i++] = KONST._NOP24;
            ProgMemWrScript.Script[i++] = KONST._NOP24;
            ProgMemWrScript.Script[i++] = KONST._COREINST24;  //TBLWTH W1, [W7++]
            ProgMemWrScript.Script[i++] = 0x81;
            ProgMemWrScript.Script[i++] = 0x9B;
            ProgMemWrScript.Script[i++] = 0xBB;
            ProgMemWrScript.Script[i++] = KONST._NOP24;
            ProgMemWrScript.Script[i++] = KONST._NOP24;
            ProgMemWrScript.Script[i++] = KONST._LOOP;
            ProgMemWrScript.Script[i++] = 0x10;
            ProgMemWrScript.Script[i++] = 0x01;
            ProgMemWrScript.Script[i++] = KONST._NOP24;
            ProgMemWrScript.Script[i++] = KONST._COREINST24;  // MOV W4, NVMKEY
            ProgMemWrScript.Script[i++] = 0x74;
            ProgMemWrScript.Script[i++] = 0x39;
            ProgMemWrScript.Script[i++] = 0x88;
            ProgMemWrScript.Script[i++] = KONST._NOP24;   //timijk: this NOP is required
            ProgMemWrScript.Script[i++] = KONST._COREINST24;   // MOV W5, NVMKEY
            ProgMemWrScript.Script[i++] = 0x75;
            ProgMemWrScript.Script[i++] = 0x39;
            ProgMemWrScript.Script[i++] = 0x88;
            ProgMemWrScript.Script[i++] = KONST._COREINST24;    // BSET 0xNVCOM, #WR
            ProgMemWrScript.Script[i++] = 0x29;
            ProgMemWrScript.Script[i++] = 0xE7;
            ProgMemWrScript.Script[i++] = 0xA8;
            ProgMemWrScript.Script[i++] = KONST._NOP24;
            ProgMemWrScript.Script[i++] = KONST._NOP24;
            ProgMemWrScript.Script[i++] = KONST._NOP24;
            ProgMemWrScript.Script[i++] = KONST._NOP24;
            ProgMemWrScript.Script[i++] = KONST._NOP24;
            ProgMemWrScript.Script[i++] = KONST._DELAY_SHORT;
            ProgMemWrScript.Script[i++] = 0x06;
            ProgMemWrScript.Script[i++] = KONST._NOP24;
            ProgMemWrScript.Script[i++] = KONST._NOP24;
            ProgMemWrScript.Script[i++] = KONST._NOP24;
            ProgMemWrScript.Script[i++] = KONST._COREINST24;    // GOTO 0x200
            ProgMemWrScript.Script[i++] = 0x00;
            ProgMemWrScript.Script[i++] = 0x02;
            ProgMemWrScript.Script[i++] = 0x04;
            ProgMemWrScript.Script[i++] = KONST._NOP24;
            ProgMemWrScript.Script[i++] = KONST._NOP24;
            ProgMemWrScript.Script[i++] = KONST._NOP24;
            ProgMemWrScript.Script[i++] = KONST._COREINST24;    // ADD W8,[W9],[W9]
            ProgMemWrScript.Script[i++] = 0x99;
            ProgMemWrScript.Script[i++] = 0x0C;
            ProgMemWrScript.Script[i++] = 0x44;
            ProgMemWrScript.Script[i++] = KONST._NOP24;   // NOP24 required
            ProgMemWrScript.Script[i++] = KONST._NOP24;   // NOP24 required

            ProgMemWrScript.ScriptLength = i;
        }

        public static void initProgMemWrAuxScript()
        {
            ushort i = 0;
            ProgMemWrAuxScript.ScriptNumber = 288;
            ProgMemWrAuxScript.ScriptName = "Prog Memory Write Aux";
            ProgMemWrAuxScript.Comment = "..";
            ProgMemWrAuxScript.ScriptVersion = 0;
            ProgMemWrAuxScript.UNUSED1 = 0;
            ProgMemWrAuxScript.Script = new ushort[10];

            ProgMemWrAuxScript.Script[i++] = KONST._NOP24;
            ProgMemWrAuxScript.Script[i++] = KONST._NOP24;
            ProgMemWrAuxScript.Script[i++] = KONST._NOP24;
            ProgMemWrAuxScript.Script[i++] = KONST._COREINST24;  // GOTO 0x200
            ProgMemWrAuxScript.Script[i++] = 0x00;
            ProgMemWrAuxScript.Script[i++] = 0x02;
            ProgMemWrAuxScript.Script[i++] = 0x04;
            ProgMemWrAuxScript.Script[i++] = KONST._NOP24;
            ProgMemWrAuxScript.Script[i++] = KONST._NOP24;
            ProgMemWrAuxScript.Script[i++] = KONST._NOP24;
            //ProgMemWrAuxScript.Script[i++] = KONST._COREINST24;   // MOV W8, NVMADR
            //ProgMemWrAuxScript.Script[i++] = 0x58;
            //ProgMemWrAuxScript.Script[i++] = 0x39;
            //ProgMemWrAuxScript.Script[i++] = 0x88;

            ProgMemWrAuxScript.ScriptLength = i;
        }

        public static void initProgMemWrPrepScript()
        {
            ushort i = 0;
            ProgMemWrPrepScript.ScriptNumber = 286;
            ProgMemWrPrepScript.ScriptName = "ProgMemory Write Prep";
            ProgMemWrPrepScript.Comment = "..";
            ProgMemWrPrepScript.ScriptVersion = 0;
            ProgMemWrPrepScript.UNUSED1 = 0;
            ProgMemWrPrepScript.Script = new ushort[58];

            ProgMemWrPrepScript.Script[i++] = KONST._NOP24;
            ProgMemWrPrepScript.Script[i++] = KONST._NOP24;
            ProgMemWrPrepScript.Script[i++] = KONST._NOP24;
            ProgMemWrPrepScript.Script[i++] = KONST._COREINST24;
            ProgMemWrPrepScript.Script[i++] = 0x00;
            ProgMemWrPrepScript.Script[i++] = 0x02;
            ProgMemWrPrepScript.Script[i++] = 0x04;
            ProgMemWrPrepScript.Script[i++] = KONST._NOP24;
            ProgMemWrPrepScript.Script[i++] = KONST._NOP24;
            ProgMemWrPrepScript.Script[i++] = KONST._NOP24;
            ProgMemWrPrepScript.Script[i++] = KONST._COREINST24;   // MOV #0x4001, W10
            ProgMemWrPrepScript.Script[i++] = 0x1A;
            ProgMemWrPrepScript.Script[i++] = 0x00;
            ProgMemWrPrepScript.Script[i++] = 0x24;
            ProgMemWrPrepScript.Script[i++] = KONST._NOP24;
            ProgMemWrPrepScript.Script[i++] = KONST._COREINST24;   // MOV W10, NVWCON
            ProgMemWrPrepScript.Script[i++] = 0x4A;
            ProgMemWrPrepScript.Script[i++] = 0x39;
            ProgMemWrPrepScript.Script[i++] = 0x88;
            ProgMemWrPrepScript.Script[i++] = KONST._NOP24;
            ProgMemWrPrepScript.Script[i++] = KONST._NOP24;
            ProgMemWrPrepScript.Script[i++] = KONST._COREINST24;  // MOV #0x55, W4
            ProgMemWrPrepScript.Script[i++] = 0x54;
            ProgMemWrPrepScript.Script[i++] = 0x05;
            ProgMemWrPrepScript.Script[i++] = 0x20;
            ProgMemWrPrepScript.Script[i++] = KONST._COREINST24;  // MOV #0xAA, W5
            ProgMemWrPrepScript.Script[i++] = 0xA5;
            ProgMemWrPrepScript.Script[i++] = 0x0A;
            ProgMemWrPrepScript.Script[i++] = 0x20;
            ProgMemWrPrepScript.Script[i++] = KONST._COREINST24;   // MOV #0xFA, W0
            ProgMemWrPrepScript.Script[i++] = 0xA0;
            ProgMemWrPrepScript.Script[i++] = 0x0F;
            ProgMemWrPrepScript.Script[i++] = 0x20;
            ProgMemWrPrepScript.Script[i++] = KONST._COREINST24;   // MOV W0, TBLPAG  
            ProgMemWrPrepScript.Script[i++] = 0xA0;
            ProgMemWrPrepScript.Script[i++] = 0x02;
            ProgMemWrPrepScript.Script[i++] = 0x88;
            ProgMemWrPrepScript.Script[i++] = KONST._WRITE_BUFWORD_W;  //W11
            ProgMemWrPrepScript.Script[i++] = 0x0B;
            ProgMemWrPrepScript.Script[i++] = KONST._WRITE_BUFBYTE_W;  //W12
            ProgMemWrPrepScript.Script[i++] = 0x0C;
            ProgMemWrPrepScript.Script[i++] = KONST._COREINST24;   // MOV W12, NVMADRU
            ProgMemWrPrepScript.Script[i++] = 0x6C;
            ProgMemWrPrepScript.Script[i++] = 0x39;
            ProgMemWrPrepScript.Script[i++] = 0x88;
            ProgMemWrPrepScript.Script[i++] = KONST._COREINST24;   // MOV #0x04, W8
            ProgMemWrPrepScript.Script[i++] = 0x48;
            ProgMemWrPrepScript.Script[i++] = 0x00;
            ProgMemWrPrepScript.Script[i++] = 0x20;
            ProgMemWrPrepScript.Script[i++] = KONST._COREINST24;   // MOV #NVMADR, W9 
            ProgMemWrPrepScript.Script[i++] = 0xA9;
            ProgMemWrPrepScript.Script[i++] = 0x72;
            ProgMemWrPrepScript.Script[i++] = 0x20;
            ProgMemWrPrepScript.Script[i++] = KONST._COREINST24;   // MOV W11, [W9]
            ProgMemWrPrepScript.Script[i++] = 0x8B;
            ProgMemWrPrepScript.Script[i++] = 0x0C;
            ProgMemWrPrepScript.Script[i++] = 0x78;
            ProgMemWrPrepScript.Script[i++] = KONST._NOP24;

            ProgMemWrPrepScript.ScriptLength = i;

        }

        public static void initConfigRdScript()
        {

            ushort i = 0;
            ConfigRdScript.ScriptNumber = 284;
            ConfigRdScript.ScriptName = "Config Read";
            ConfigRdScript.Comment = "..";
            ConfigRdScript.ScriptVersion = 0;
            ConfigRdScript.UNUSED1 = 0;
            ConfigRdScript.Script = new ushort[57];

            ConfigRdScript.Script[i++] = KONST._NOP24;
            ConfigRdScript.Script[i++] = KONST._NOP24;
            ConfigRdScript.Script[i++] = KONST._NOP24;
            ConfigRdScript.Script[i++] = KONST._COREINST24;
            ConfigRdScript.Script[i++] = 0x00;
            ConfigRdScript.Script[i++] = 0x02;
            ConfigRdScript.Script[i++] = 0x04;
            ConfigRdScript.Script[i++] = KONST._NOP24;
            ConfigRdScript.Script[i++] = KONST._NOP24;
            ConfigRdScript.Script[i++] = KONST._NOP24;
            ConfigRdScript.Script[i++] = KONST._COREINST24;
            ConfigRdScript.Script[i++] = 0x20;
            ConfigRdScript.Script[i++] = 0x00;
            ConfigRdScript.Script[i++] = 0x20;
            ConfigRdScript.Script[i++] = KONST._COREINST24;
            ConfigRdScript.Script[i++] = 0xA0;
            ConfigRdScript.Script[i++] = 0x02;
            ConfigRdScript.Script[i++] = 0x88;
            ConfigRdScript.Script[i++] = KONST._COREINST24;
            ConfigRdScript.Script[i++] = 0x06;
            ConfigRdScript.Script[i++] = 0xFF;
            ConfigRdScript.Script[i++] = 0x2A;
            ConfigRdScript.Script[i++] = KONST._NOP24;
            ConfigRdScript.Script[i++] = KONST._NOP24;
            ConfigRdScript.Script[i++] = KONST._COREINST24;
            ConfigRdScript.Script[i++] = 0x07;
            ConfigRdScript.Script[i++] = 0x00;
            ConfigRdScript.Script[i++] = 0x20;
            ConfigRdScript.Script[i++] = KONST._NOP24;
            ConfigRdScript.Script[i++] = KONST._COREINST24;
            ConfigRdScript.Script[i++] = 0xB6;
            ConfigRdScript.Script[i++] = 0x0B;
            ConfigRdScript.Script[i++] = 0xBA;
            ConfigRdScript.Script[i++] = KONST._NOP24;
            ConfigRdScript.Script[i++] = KONST._NOP24;
            ConfigRdScript.Script[i++] = KONST._NOP24;
            ConfigRdScript.Script[i++] = KONST._NOP24;
            ConfigRdScript.Script[i++] = KONST._NOP24;
            ConfigRdScript.Script[i++] = KONST._COREINST24;
            ConfigRdScript.Script[i++] = 0x40;
            ConfigRdScript.Script[i++] = 0x7C;
            ConfigRdScript.Script[i++] = 0x88;
            ConfigRdScript.Script[i++] = KONST._NOP24;
            ConfigRdScript.Script[i++] = KONST._VISI24;
            ConfigRdScript.Script[i++] = KONST._NOP24;
            ConfigRdScript.Script[i++] = KONST._LOOP;
            ConfigRdScript.Script[i++] = 0x10;
            ConfigRdScript.Script[i++] = 0x05;
            ConfigRdScript.Script[i++] = KONST._NOP24;
            ConfigRdScript.Script[i++] = KONST._NOP24;
            ConfigRdScript.Script[i++] = KONST._COREINST24;
            ConfigRdScript.Script[i++] = 0x00;
            ConfigRdScript.Script[i++] = 0x02;
            ConfigRdScript.Script[i++] = 0x04;
            ConfigRdScript.Script[i++] = KONST._NOP24;
            ConfigRdScript.Script[i++] = KONST._NOP24;
            ConfigRdScript.Script[i++] = KONST._NOP24;

            ConfigRdScript.ScriptLength = i;

        }

        public static void initUserIDRdScript()
        {
            ushort i = 0;
            UserIDRdScript.ScriptNumber = 285;
            UserIDRdScript.ScriptName = "User ID Read";
            UserIDRdScript.Comment = "..";
            UserIDRdScript.ScriptVersion = 0;
            UserIDRdScript.UNUSED1 = 0;
            UserIDRdScript.Script = new ushort[53];

            UserIDRdScript.Script[i++] = KONST._NOP24;
            UserIDRdScript.Script[i++] = KONST._NOP24;
            UserIDRdScript.Script[i++] = KONST._NOP24;
            UserIDRdScript.Script[i++] = KONST._COREINST24;
            UserIDRdScript.Script[i++] = 0x00;
            UserIDRdScript.Script[i++] = 0x02;
            UserIDRdScript.Script[i++] = 0x04;
            UserIDRdScript.Script[i++] = KONST._NOP24;
            UserIDRdScript.Script[i++] = KONST._NOP24;
            UserIDRdScript.Script[i++] = KONST._NOP24;
            UserIDRdScript.Script[i++] = KONST._COREINST24;
            UserIDRdScript.Script[i++] = 0x00;
            UserIDRdScript.Script[i++] = 0x08;
            UserIDRdScript.Script[i++] = 0x20;
            UserIDRdScript.Script[i++] = KONST._COREINST24;
            UserIDRdScript.Script[i++] = 0xA0;
            UserIDRdScript.Script[i++] = 0x02;
            UserIDRdScript.Script[i++] = 0x88;
            UserIDRdScript.Script[i++] = KONST._COREINST24;
            UserIDRdScript.Script[i++] = 0x86;
            UserIDRdScript.Script[i++] = 0xFF;
            UserIDRdScript.Script[i++] = 0x20;
            UserIDRdScript.Script[i++] = KONST._NOP24;
            UserIDRdScript.Script[i++] = KONST._NOP24;
            UserIDRdScript.Script[i++] = KONST._COREINST24;
            UserIDRdScript.Script[i++] = 0x07;
            UserIDRdScript.Script[i++] = 0x00;
            UserIDRdScript.Script[i++] = 0x20;
            UserIDRdScript.Script[i++] = KONST._NOP24;
            UserIDRdScript.Script[i++] = KONST._COREINST24;
            UserIDRdScript.Script[i++] = 0xB6;
            UserIDRdScript.Script[i++] = 0x0B;
            UserIDRdScript.Script[i++] = 0xBA;
            UserIDRdScript.Script[i++] = KONST._NOP24;
            UserIDRdScript.Script[i++] = KONST._NOP24;
            UserIDRdScript.Script[i++] = KONST._NOP24;
            UserIDRdScript.Script[i++] = KONST._NOP24;
            UserIDRdScript.Script[i++] = KONST._NOP24;
            UserIDRdScript.Script[i++] = KONST._COREINST24;
            UserIDRdScript.Script[i++] = 0x40;
            UserIDRdScript.Script[i++] = 0x7C;
            UserIDRdScript.Script[i++] = 0x88;
            UserIDRdScript.Script[i++] = KONST._NOP24;
            UserIDRdScript.Script[i++] = KONST._WRITE_BITS_LITERAL;
            UserIDRdScript.Script[i++] = 0x04;
            UserIDRdScript.Script[i++] = 0x01;
            UserIDRdScript.Script[i++] = KONST._WRITE_BYTE_LITERAL;
            UserIDRdScript.Script[i++] = 0x00;
            UserIDRdScript.Script[i++] = KONST._RD2_BYTE_BUFFER;
            UserIDRdScript.Script[i++] = KONST._READ_BYTE;
            UserIDRdScript.Script[i++] = KONST._LOOP;
            UserIDRdScript.Script[i++] = 0x16;
            UserIDRdScript.Script[i++] = 0x03;

            UserIDRdScript.ScriptLength = i;

        }

        public static void initProgMemAddrSetScript()
        {
            ushort i = 0;
            ProgMemAddrSetScript.ScriptNumber = 281;
            ProgMemAddrSetScript.ScriptName = "Addr Set";
            ProgMemAddrSetScript.Comment = "..";
            ProgMemAddrSetScript.ScriptVersion = 0;
            ProgMemAddrSetScript.UNUSED1 = 0;
            ProgMemAddrSetScript.Script = new ushort[23];

            ProgMemAddrSetScript.Script[i++] = KONST._NOP24;
            ProgMemAddrSetScript.Script[i++] = KONST._NOP24;
            ProgMemAddrSetScript.Script[i++] = KONST._NOP24;
            ProgMemAddrSetScript.Script[i++] = KONST._COREINST24;
            ProgMemAddrSetScript.Script[i++] = 0x00;
            ProgMemAddrSetScript.Script[i++] = 0x02;
            ProgMemAddrSetScript.Script[i++] = 0x04;
            ProgMemAddrSetScript.Script[i++] = KONST._NOP24;
            ProgMemAddrSetScript.Script[i++] = KONST._NOP24;
            ProgMemAddrSetScript.Script[i++] = KONST._NOP24;
            ProgMemAddrSetScript.Script[i++] = KONST._WRITE_BUFWORD_W;  // MOV BUF, W6
            ProgMemAddrSetScript.Script[i++] = 0x06;
            ProgMemAddrSetScript.Script[i++] = KONST._WRITE_BUFBYTE_W;  // MOV BUF, W0
            ProgMemAddrSetScript.Script[i++] = 0x00;
            ProgMemAddrSetScript.Script[i++] = KONST._COREINST24;    // MOV W0, TBLPAG
            ProgMemAddrSetScript.Script[i++] = 0xA0;
            ProgMemAddrSetScript.Script[i++] = 0x02;
            ProgMemAddrSetScript.Script[i++] = 0x88;
            ProgMemAddrSetScript.Script[i++] = KONST._COREINST24;    // MOV #VISI, W7
            ProgMemAddrSetScript.Script[i++] = 0x87;
            ProgMemAddrSetScript.Script[i++] = 0xF8;
            ProgMemAddrSetScript.Script[i++] = 0x20;
            ProgMemAddrSetScript.Script[i++] = KONST._NOP24;

            ProgMemAddrSetScript.ScriptLength = i;
        }

        public static void initProgMemRdScript()
        {
            ushort i = 0;
            ProgMemRdScript.ScriptNumber = 282;
            ProgMemRdScript.ScriptName = "Prog Mem Read";
            ProgMemRdScript.Comment = "..";
            ProgMemRdScript.ScriptVersion = 0;
            ProgMemRdScript.UNUSED1 = 0;
            ProgMemRdScript.Script = new ushort[39];

            ProgMemRdScript.Script[i++] = KONST._COREINST24;   // TBLRDL [W6],[W7]?
            ProgMemRdScript.Script[i++] = 0x96;
            ProgMemRdScript.Script[i++] = 0x0B;
            ProgMemRdScript.Script[i++] = 0xBA;
            ProgMemRdScript.Script[i++] = KONST._NOP24;
            ProgMemRdScript.Script[i++] = KONST._NOP24;
            ProgMemRdScript.Script[i++] = KONST._NOP24;
            ProgMemRdScript.Script[i++] = KONST._NOP24;
            ProgMemRdScript.Script[i++] = KONST._NOP24;
            ProgMemRdScript.Script[i++] = KONST._VISI24;
            ProgMemRdScript.Script[i++] = KONST._COREINST24;   // TBLRDH [W6++],[W7]
            ProgMemRdScript.Script[i++] = 0xB6;
            ProgMemRdScript.Script[i++] = 0x8B;
            ProgMemRdScript.Script[i++] = 0xBA;
            ProgMemRdScript.Script[i++] = KONST._NOP24;
            ProgMemRdScript.Script[i++] = KONST._NOP24;
            ProgMemRdScript.Script[i++] = KONST._NOP24;
            ProgMemRdScript.Script[i++] = KONST._NOP24;
            ProgMemRdScript.Script[i++] = KONST._NOP24;
            ProgMemRdScript.Script[i++] = KONST._WRITE_BITS_LITERAL;
            ProgMemRdScript.Script[i++] = 0x04;
            ProgMemRdScript.Script[i++] = 0x01;
            ProgMemRdScript.Script[i++] = KONST._WRITE_BYTE_LITERAL;
            ProgMemRdScript.Script[i++] = 0x00;
            ProgMemRdScript.Script[i++] = KONST._RD2_BYTE_BUFFER;   //READ 1 byte to buffer
            ProgMemRdScript.Script[i++] = KONST._READ_BYTE;         //READ 1 byte and throw it away
            ProgMemRdScript.Script[i++] = KONST._LOOP;
            ProgMemRdScript.Script[i++] = 0x1A;
            ProgMemRdScript.Script[i++] = 0x1F;
            ProgMemRdScript.Script[i++] = KONST._NOP24;
            ProgMemRdScript.Script[i++] = KONST._NOP24;
            ProgMemRdScript.Script[i++] = KONST._NOP24;
            ProgMemRdScript.Script[i++] = KONST._COREINST24;
            ProgMemRdScript.Script[i++] = 0x00;
            ProgMemRdScript.Script[i++] = 0x02;
            ProgMemRdScript.Script[i++] = 0x04;
            ProgMemRdScript.Script[i++] = KONST._NOP24;
            ProgMemRdScript.Script[i++] = KONST._NOP24;
            ProgMemRdScript.Script[i++] = KONST._NOP24;

            ProgMemRdScript.ScriptLength = i;
        }

        public static void initChipEraseScript()
        {
            ushort i = 0;
            ChipEraseScript.ScriptNumber = 280;
            ChipEraseScript.ScriptName = "Chip Erase User Memory";
            ChipEraseScript.Comment = "..";
            ChipEraseScript.ScriptVersion = 0;
            ChipEraseScript.UNUSED1 = 0;
            ChipEraseScript.Script = new ushort[45];
            ChipEraseScript.Script[i++] = KONST._NOP24;
            ChipEraseScript.Script[i++] = KONST._NOP24;
            ChipEraseScript.Script[i++] = KONST._NOP24;
            ChipEraseScript.Script[i++] = KONST._COREINST24;
            ChipEraseScript.Script[i++] = 0x00;
            ChipEraseScript.Script[i++] = 0x02;
            ChipEraseScript.Script[i++] = 0x04;
            ChipEraseScript.Script[i++] = KONST._NOP24;
            ChipEraseScript.Script[i++] = KONST._NOP24;
            ChipEraseScript.Script[i++] = KONST._NOP24;
            ChipEraseScript.Script[i++] = KONST._COREINST24;  //MOV #0x400D,W10; User Memory and Configuration Bits
            ChipEraseScript.Script[i++] = 0xDA;
            ChipEraseScript.Script[i++] = 0x00;
            ChipEraseScript.Script[i++] = 0x24;
            ChipEraseScript.Script[i++] = KONST._COREINST24;
            ChipEraseScript.Script[i++] = 0x4A;
            ChipEraseScript.Script[i++] = 0x39;
            ChipEraseScript.Script[i++] = 0x88;
            ChipEraseScript.Script[i++] = KONST._NOP24;
            ChipEraseScript.Script[i++] = KONST._NOP24;
            ChipEraseScript.Script[i++] = KONST._COREINST24;
            ChipEraseScript.Script[i++] = 0x51;
            ChipEraseScript.Script[i++] = 0x05;
            ChipEraseScript.Script[i++] = 0x20;
            ChipEraseScript.Script[i++] = KONST._COREINST24;
            ChipEraseScript.Script[i++] = 0x71;
            ChipEraseScript.Script[i++] = 0x39;
            ChipEraseScript.Script[i++] = 0x88;
            ChipEraseScript.Script[i++] = KONST._COREINST24;
            ChipEraseScript.Script[i++] = 0xA1;
            ChipEraseScript.Script[i++] = 0x0A;
            ChipEraseScript.Script[i++] = 0x20;
            ChipEraseScript.Script[i++] = KONST._COREINST24;
            ChipEraseScript.Script[i++] = 0x71;
            ChipEraseScript.Script[i++] = 0x39;
            ChipEraseScript.Script[i++] = 0x88;
            ChipEraseScript.Script[i++] = KONST._COREINST24;
            ChipEraseScript.Script[i++] = 0x29;
            ChipEraseScript.Script[i++] = 0xE7;
            ChipEraseScript.Script[i++] = 0xA8;
            ChipEraseScript.Script[i++] = KONST._NOP24;
            ChipEraseScript.Script[i++] = KONST._NOP24;
            ChipEraseScript.Script[i++] = KONST._NOP24;
            ChipEraseScript.Script[i++] = KONST._DELAY_LONG;
            ChipEraseScript.Script[i++] = 0x50;

            ChipEraseScript.ScriptLength = i;

        }

        public static void initConfigWrScript()
        {

            ushort i = 0;
            ConfigWrScript.ScriptNumber = 286;
            ConfigWrScript.ScriptName = "Config Memory Write";
            ConfigWrScript.Comment = "..";
            ConfigWrScript.ScriptVersion = 0;
            ConfigWrScript.UNUSED1 = 0;
            ConfigWrScript.Script = new ushort[59];

            ConfigWrScript.Script[i++] = KONST._COREINST24;  //CLR W7
            ConfigWrScript.Script[i++] = 0x80;
            ConfigWrScript.Script[i++] = 0x03;
            ConfigWrScript.Script[i++] = 0xEB;
            ConfigWrScript.Script[i++] = KONST._WRITE_BUFWORD_W;  //MOV BUF, W0
            ConfigWrScript.Script[i++] = 0x00;
            ConfigWrScript.Script[i++] = KONST._WRITE_BUFWORD_W;  //MOV BUF, W1
            ConfigWrScript.Script[i++] = 0x01;
            ConfigWrScript.Script[i++] = KONST._COREINST24;  //TBLWTL W0,[W7++]
            ConfigWrScript.Script[i++] = 0x80;
            ConfigWrScript.Script[i++] = 0x1B;
            ConfigWrScript.Script[i++] = 0xBB;
            ConfigWrScript.Script[i++] = KONST._NOP24;
            ConfigWrScript.Script[i++] = KONST._NOP24;
            ConfigWrScript.Script[i++] = KONST._COREINST24;  //TBLWTL W1, [W7]
            ConfigWrScript.Script[i++] = 0x81;
            ConfigWrScript.Script[i++] = 0x0B;
            ConfigWrScript.Script[i++] = 0xBB;
            ConfigWrScript.Script[i++] = KONST._NOP24;
            ConfigWrScript.Script[i++] = KONST._NOP24;
            ConfigWrScript.Script[i++] = KONST._COREINST24;  // MOV W4, NVMKEY
            ConfigWrScript.Script[i++] = 0x74;
            ConfigWrScript.Script[i++] = 0x39;
            ConfigWrScript.Script[i++] = 0x88;
            ConfigWrScript.Script[i++] = KONST._NOP24;   //timijk: this NOP is required
            ConfigWrScript.Script[i++] = KONST._COREINST24;   // MOV W5, NVMKEY
            ConfigWrScript.Script[i++] = 0x75;
            ConfigWrScript.Script[i++] = 0x39;
            ConfigWrScript.Script[i++] = 0x88;
            ConfigWrScript.Script[i++] = KONST._COREINST24;    // BSET 0xNVCOM, #WR
            ConfigWrScript.Script[i++] = 0x29;
            ConfigWrScript.Script[i++] = 0xE7;
            ConfigWrScript.Script[i++] = 0xA8;
            ConfigWrScript.Script[i++] = KONST._NOP24;
            ConfigWrScript.Script[i++] = KONST._NOP24;
            ConfigWrScript.Script[i++] = KONST._NOP24;
            ConfigWrScript.Script[i++] = KONST._NOP24;
            ConfigWrScript.Script[i++] = KONST._NOP24;
            ConfigWrScript.Script[i++] = KONST._DELAY_SHORT;
            ConfigWrScript.Script[i++] = 0x06;
            ConfigWrScript.Script[i++] = KONST._NOP24;
            ConfigWrScript.Script[i++] = KONST._NOP24;
            ConfigWrScript.Script[i++] = KONST._NOP24;
            ConfigWrScript.Script[i++] = KONST._COREINST24;    // GOTO 0x200
            ConfigWrScript.Script[i++] = 0x00;
            ConfigWrScript.Script[i++] = 0x02;
            ConfigWrScript.Script[i++] = 0x04;
            ConfigWrScript.Script[i++] = KONST._NOP24;
            ConfigWrScript.Script[i++] = KONST._NOP24;
            ConfigWrScript.Script[i++] = KONST._NOP24;
            ConfigWrScript.Script[i++] = KONST._COREINST24;    // ADD W8,[W9],[W9]
            ConfigWrScript.Script[i++] = 0x99;
            ConfigWrScript.Script[i++] = 0x0C;
            ConfigWrScript.Script[i++] = 0x44;
            ConfigWrScript.Script[i++] = KONST._NOP24;   // NOP24 required
            ConfigWrScript.Script[i++] = KONST._NOP24;   // NOP24 required
            ConfigWrScript.Script[i++] = KONST._LOOP;
            ConfigWrScript.Script[i++] = 56;
            ConfigWrScript.Script[i++] = 0x02;

            ConfigWrScript.ScriptLength = i;


        }

        // use ProgMemWrPrepScript to set the address
        // 1 page = 1024 instructions
        // PE at 0x800000 has 2 pages.
        public static void initDebugWriteVectorScript()
        {
            ushort i = 0;
            DebugWriteVectorScript.ScriptNumber = 280;
            DebugWriteVectorScript.ScriptName = "Erase PE and User ID";
            DebugWriteVectorScript.Comment = "..";
            DebugWriteVectorScript.ScriptVersion = 0;
            DebugWriteVectorScript.UNUSED1 = 0;
            DebugWriteVectorScript.Script = new ushort[47];
            DebugWriteVectorScript.Script[i++] = KONST._COREINST24;  //MOV #0x4003,W10; PE at 0x800000
            DebugWriteVectorScript.Script[i++] = 0x3A;
            DebugWriteVectorScript.Script[i++] = 0x00;
            DebugWriteVectorScript.Script[i++] = 0x24;
            DebugWriteVectorScript.Script[i++] = KONST._COREINST24;  //MOV W10, NVMCON
            DebugWriteVectorScript.Script[i++] = 0x4A;
            DebugWriteVectorScript.Script[i++] = 0x39;
            DebugWriteVectorScript.Script[i++] = 0x88;
            DebugWriteVectorScript.Script[i++] = KONST._NOP24;
            DebugWriteVectorScript.Script[i++] = KONST._NOP24;
            DebugWriteVectorScript.Script[i++] = KONST._COREINST24;  // MOV W4, NVMKEY
            DebugWriteVectorScript.Script[i++] = 0x74;
            DebugWriteVectorScript.Script[i++] = 0x39;
            DebugWriteVectorScript.Script[i++] = 0x88;
            DebugWriteVectorScript.Script[i++] = KONST._NOP24;
            DebugWriteVectorScript.Script[i++] = KONST._COREINST24;  // MOV W5, NVMKEY
            DebugWriteVectorScript.Script[i++] = 0x75;
            DebugWriteVectorScript.Script[i++] = 0x39;
            DebugWriteVectorScript.Script[i++] = 0x88;
            DebugWriteVectorScript.Script[i++] = KONST._COREINST24;   // BSET NVMCON, #WR
            DebugWriteVectorScript.Script[i++] = 0x29;
            DebugWriteVectorScript.Script[i++] = 0xE7;
            DebugWriteVectorScript.Script[i++] = 0xA8;
            DebugWriteVectorScript.Script[i++] = KONST._NOP24;
            DebugWriteVectorScript.Script[i++] = KONST._NOP24;
            DebugWriteVectorScript.Script[i++] = KONST._NOP24;
            DebugWriteVectorScript.Script[i++] = KONST._DELAY_LONG;
            DebugWriteVectorScript.Script[i++] = 0x50;
            DebugWriteVectorScript.Script[i++] = KONST._NOP24;
            DebugWriteVectorScript.Script[i++] = KONST._NOP24;
            DebugWriteVectorScript.Script[i++] = KONST._NOP24;
            DebugWriteVectorScript.Script[i++] = KONST._COREINST24;    // GOTO 0x200
            DebugWriteVectorScript.Script[i++] = 0x00;
            DebugWriteVectorScript.Script[i++] = 0x02;
            DebugWriteVectorScript.Script[i++] = 0x04;
            DebugWriteVectorScript.Script[i++] = KONST._NOP24;
            DebugWriteVectorScript.Script[i++] = KONST._NOP24;
            DebugWriteVectorScript.Script[i++] = KONST._NOP24;
            DebugWriteVectorScript.Script[i++] = KONST._COREINST24;    // BSET NVMADR, #0x0B
            DebugWriteVectorScript.Script[i++] = 0x2B;
            DebugWriteVectorScript.Script[i++] = 0x67;
            DebugWriteVectorScript.Script[i++] = 0xA8;
            DebugWriteVectorScript.Script[i++] = KONST._NOP24;
            DebugWriteVectorScript.Script[i++] = KONST._NOP24;
            DebugWriteVectorScript.Script[i++] = KONST._LOOP;
            DebugWriteVectorScript.Script[i++] = 34;
            DebugWriteVectorScript.Script[i++] = 0x01;

            DebugWriteVectorScript.ScriptLength = i;

        }

    }
}

﻿using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;
using Aura_OS.System.Graphics;
using Aura_OS.System.Shell.cmdIntr;
using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.HAL;
using Cosmos.System;
using Cosmos.System.ExtendedASCII;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using Aura_OS.Processing;
using System.Drawing;
using Aura_OS.System;
using Aura_OS.Interpreter;
using Aura_OS.Application.GameBoyEmu;
using Aura_OS.Graphics;

namespace Aura_OS
{
    public class Kernel
    {
        public static string ComputerName = "kitsune";
        public static string userLogged = "root";
        public static string userLevelLogged = "admin";
        public static bool Running = false;
        public static string Version = "1";
        public static string Revision = VersionInfo.revision;
        public static string langSelected = "pt_PT";
        public static string BootTime = "01/01/1970";

        public static string CurrentVolume = @"0:\";
        public static string CurrentDirectory = @"0:\";

        //FILES
        public static Bitmap programIco;
        public static Bitmap terminalIco;
        public static Bitmap powerIco;
        public static Bitmap connectedIco;

        public static Bitmap programLogo;
        public static Bitmap errorLogo;

        public static Bitmap AuraLogo;
        public static Bitmap CosmosLogo;

        public static Bitmap wallpaper;

        public static Bitmap cursor;
        public static PCScreenFont font;
        public static PCScreenFont fontTerminal;

        //GRAPHICS
        public static uint screenWidth = 1024;
        public static uint screenHeight = 768;

        public static Canvas canvas;
        public static Pen WhitePen = new Pen(Color.White);
        public static Pen BlackPen = new Pen(Color.Black);
        public static Pen avgColPen = new Pen(Color.PowderBlue);
        public static Dock dock;

        //PROCESSES
        public static ProcessManager ProcessManager;

        public static WindowManager WindowManager;

        public static Terminal console;
        public static MemoryInfo memoryInfo;
        public static SystemInfo systemInfo;
        public static GameBoyEmu gameBoyEmu;

        public static CommandManager CommandManager;

        public static bool Pressed;
        public static int FreeCount = 0;

        private static int _frames = 0;
        public static int _fps = 0;
        public static int _deltaT = 0;

        public static CosmosVFS VirtualFileSystem = new CosmosVFS();
        public static Dictionary<string, string> EnvironmentVariables = new Dictionary<string, string>();

        public static void BeforeRun()
        {
            //Start Filesystem
            VFSManager.RegisterVFS(VirtualFileSystem);

            //Load Localization
            System.CustomConsole.WriteLineInfo("Initializing localization...");

            Encoding.RegisterProvider(CosmosEncodingProvider.Instance);
            KeyboardManager.SetKeyLayout(new Sys.ScanMaps.DE_Standard());

            LoadFiles();

            CustomConsole.WriteLineInfo("Starting Canvas...");

            //START GRAPHICS
            canvas = FullScreenCanvas.GetFullScreenCanvas(new Mode((int)screenWidth, (int)screenHeight, ColorDepth.ColorDepth32));
            dock = new Dock();

            //START PROCESSES
            ProcessManager = new ProcessManager();
            ProcessManager.Initialize();

            CommandManager = new CommandManager();
            CommandManager.Initialize();

            console = new Terminal(700, 600, 40, 40);
            console.Initialize();

            memoryInfo = new MemoryInfo(400, 300, 40, 40);
            memoryInfo.Initialize();

            systemInfo = new SystemInfo(402, 197, 40, 40);
            systemInfo.Initialize();

            gameBoyEmu = new GameBoyEmu(160 + 2, 144 + 20, 40, 40);
            gameBoyEmu.Initialize();

            WindowManager = new WindowManager();
            WindowManager.Initialize();

            //START MOUSE
            MouseManager.ScreenWidth = screenWidth;
            MouseManager.ScreenHeight = screenHeight;

            BootTime = Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true);

            Running = true;
        }

        public static void LoadFiles()
        {
            System.CustomConsole.WriteLineInfo("Loading files...");

            //LOAD FILES

            programIco = new Bitmap(Files.NoIcon);

            System.CustomConsole.WriteLineOK("Program icon.");

            terminalIco = new Bitmap(Files.TerminalIcon);

            System.CustomConsole.WriteLineOK("Terminal icon.");

            powerIco = new Bitmap(Files.PowerIcon);

            System.CustomConsole.WriteLineOK("Power icon.");

            connectedIco = new Bitmap(Files.ConnectedIcon);

            System.CustomConsole.WriteLineOK("Connected icon.");

            AuraLogo = new Bitmap(Files.AuraImage);

            System.CustomConsole.WriteLineOK("logo.");

            CosmosLogo = new Bitmap(Files.CosmosLogo);

            System.CustomConsole.WriteLineOK("CosmosLogo logo.");

            wallpaper = new Bitmap(Files.Wallpaper);

            System.CustomConsole.WriteLineOK("Wallpaper.");

            errorLogo = new Bitmap(Files.ErrorImage);

            System.CustomConsole.WriteLineOK("Error logo.");

            programLogo = new Bitmap(Files.ProgramImage);

            System.CustomConsole.WriteLineOK("Program icon 2.");

            cursor = new Bitmap(Files.CursorIcon);

            System.CustomConsole.WriteLineOK("Cursor.");

            font = PCScreenFont.LoadFont(Files.Font);

            System.CustomConsole.WriteLineOK("Font.");
        }

        public static void Run()
        {
            try
            {
                if (_deltaT != RTC.Second)
                {
                    _fps = _frames;
                    _frames = 0;
                    _deltaT = RTC.Second;
                }

                _frames++;

                FreeCount = Heap.Collect();

                switch (MouseManager.MouseState)
                {
                    case MouseState.Left:
                        Pressed = true;
                        break;
                    case MouseState.None:
                        Pressed = false;
                        break;
                }

                //canvas.Clear(0x000000);

                canvas.DrawImage(wallpaper, 0, 0);

                //canvas.DrawImage(bootBitmap, (int)(screenWidth / 2 - bootBitmap.Width / 2), (int)(screenHeight / 2 - bootBitmap.Height / 2));

                canvas.DrawString("fps=" + _fps, font, WhitePen, 2, (int)screenHeight - (font.Height * 2));
                canvas.DrawString("KitsuneROM [" + Version + "." + Revision + "]", font, WhitePen, 2, (int)screenHeight - font.Height);

                WindowManager.DrawWindows();

                dock.Update();

                DrawCursor(MouseManager.X, MouseManager.Y);

                canvas.Display();
            }
            catch (Exception ex)
            {
                System.Crash.WriteException(ex);
            }
        }

        public static void DrawCursor(uint x, uint y)
        {
            canvas.DrawImageAlpha(cursor, (int)x, (int)y);
        }
    }
}

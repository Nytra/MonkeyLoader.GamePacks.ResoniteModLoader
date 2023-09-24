﻿using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MonoMod.Logs;
using MonoMod.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Doorstop
{
    internal static class Entrypoint
    {
        public static void HelloMethod()
        {
            File.WriteAllText("test.log", "Hello from new FrooxEngine.Engine static constructor");
        }

        public static void Start()
        {
            Debugger.Break();

            File.Delete("error.log");
            File.Delete("test.log");

            try
            {
                var frooxEngine = AssemblyDefinition.ReadAssembly("Resonite_Data\\Managed\\FrooxEngine.dll");

                DebugLog.Log("MonkeyLoader", LogLevel.Trace, string.Join(", ", frooxEngine.Modules.Select(m => m.Name)));

                var engine = frooxEngine.MainModule.Types.FirstOrDefault(t => t.Name == "Engine");
                var engineCCtor = engine.GetStaticConstructor();

                var processor = engineCCtor.Body.GetILProcessor();
                processor.InsertBefore(engineCCtor.Body.Instructions.First(), processor.Create(OpCodes.Call, typeof(Entrypoint).GetMethod(nameof(HelloMethod))));

                var ms = new MemoryStream();
                frooxEngine.Write(ms);

                Assembly.Load(ms.ToArray());

                DebugLog.Log("MonkeyLoader", LogLevel.Trace, "Loaded FrooxEngine from Memory");
            }
            catch (Exception ex)
            {
                File.WriteAllText("error.log", ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }
    }
}
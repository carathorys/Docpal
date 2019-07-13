﻿using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Docpal
{
	class Program
	{
		static void Main(string[] args)
		{
			var paths = Args.GetPaths();
			if (paths.Length == 0 || Args.Help)
			{
				Console.WriteLine($"Usage: {AppDomain.CurrentDomain.FriendlyName} [OPTIONS]... <path_to_dll> -out <output_folder>");
                Console.WriteLine();
                Console.WriteLine($"Options:");
                Console.WriteLine($"  --mgtable    methods groups in type page will reported in a table with summary foreach method in the group");
				return;
			}

			var dllPath = Path.GetFullPath(paths[0]);
			var xmlPath = Args.Property("xml", DocUtilities.ChangeExtension(dllPath, "xml"));
			var outputPath = Args.Property("out", "docs");
			var outputDir = Path.GetDirectoryName(outputPath);
			bool ignoreXML = Args.Flag("noxml");

			// Check whether the file even exists
			if (!File.Exists(dllPath))
			{
				Console.WriteLine($"File '{dllPath}' does not exist.");
				return;
			}

			try
			{
				Console.WriteLine("Building docs...");

				var dll = Assembly.LoadFrom(dllPath);
				DocpalGenerator docpal;
				var xmlData = new XmlDocument();

				// Load XML document
				if (File.Exists(xmlPath) && !ignoreXML)
				{
					xmlData.Load(xmlPath);
					Console.WriteLine($"Found XML: {xmlPath}");
				}
				else
				{
					Console.WriteLine("No XML docs found, using only assembly...");
				}

				var xmlDocs = new ProjectXmlDocs(xmlData);				

				// Determine type of docs generator to use
				if (Args.Flag("slim"))
				{
					docpal = new DocpalSlim(xmlDocs, dll);
				}
				else
				{
					docpal = new DocpalPages(xmlDocs, dll);
				}

				Directory.CreateDirectory(outputDir);

				// Run build
				docpal.BuildDocs(outputPath);

				Console.WriteLine("Done, enjoy!");

			}
			catch (Exception e)
			{
				Console.WriteLine($"Unfortunately, there was an error.\n\n{e}");
				Environment.Exit(1);
			}
		}
	}
}

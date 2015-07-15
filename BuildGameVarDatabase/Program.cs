using System;
using System.IO;
using System.Reflection;
using Duality;
using HonourBound.Resources.GameVars;
using UtilsAndResources;

namespace BuildGameVarDatabase
{
	class Program
	{
		private static string _gamePath;

		static void Main(string[] args)
		{
			AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
			
			_gamePath = null;
			if (args.Length > 0)
				_gamePath = args[0];
			if (!DirectoryHelper.ExistsAndPathValid(_gamePath))
				return;

			BuildGameVarDatabase();
		}

		private static void BuildGameVarDatabase()
		{
			const string folderPath = "Data\\GameVars";
			var gameVarsResources = Resource.GetResourceFiles(Path.Combine(_gamePath, folderPath));

			var database =
				ContentProvider.RequestContent<GameVarsDatabase>(Path.Combine(_gamePath,
					"Data\\GameVars\\settings.GameVarsDatabase.res"));
			if (database.Res == null)
			{
				Console.WriteLine(
					"BuildGameVarDatabase: Please make sure there is a GameVarDatabase resource called 'settings' in the GameVars data folder before running this tool");
				return;
			}
			database.Res.Clear();

			foreach (var gameVarsResource in gameVarsResources)
			{
				var res = ContentProvider.RequestContent(gameVarsResource);

				if (res.Res == null || (res.Res is IGameVar) == false)
					continue;

				var variablePath = res.Res.FullName.Replace(_gamePath, "").TrimStart('\\');
				if (res.Res is IntGameVarResource)
					database.Res.AddGameVar(variablePath, ((IntGameVarResource) res.Res).Value);
				else if (res.Res is FloatGameVarResource)
					database.Res.AddGameVar(variablePath, ((FloatGameVarResource) res.Res).Value);
				else if (res.Res is ColourGameVarResource)
					database.Res.AddGameVar(variablePath, ((ColourGameVarResource)res.Res).Value);
			}

			database.Res.Save();
		}

		private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			if (args.Name.ToLower().Contains(".core"))
				return Assembly.LoadFrom(Path.Combine(_gamePath, "Plugins\\GamePlugin.core.dll"));
			
			if(args.Name.ToLower().Contains("lz4"))
				return Assembly.LoadFrom(Path.Combine(_gamePath, "lz4.dll"));

			return null;
		}
	}
}
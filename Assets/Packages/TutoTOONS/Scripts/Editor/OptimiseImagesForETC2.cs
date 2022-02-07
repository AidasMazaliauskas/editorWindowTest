using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace TutoTOONS
{
	public class MyTreeView : TreeView
	{
		public TreeViewItem root;

		public MyTreeView(TreeViewState treeViewState)
			: base(treeViewState)
		{
			Reload();
		}

		protected override TreeViewItem BuildRoot()
		{
			BuildSizeOptimizer.id = 0;

			root = new TreeViewItem { id = BuildSizeOptimizer.id++, depth = -1, displayName = "Assets" };

			var dirs = Directory.GetDirectories(Application.dataPath);

			for (int i = 0; i < dirs.Length; i++)
			{
				BuildSizeOptimizer.GetDirs(dirs[i], root);
			}

			SetupDepthsFromParentsAndChildren(root);
			
			return root;
		}
	}

	public class BuildSizeOptimizer : EditorWindow
	{
		[SerializeField] TreeViewState treeviewState;
		static MyTreeView myTreeView;
		static bool limit1024, forceNormalCompression;
		static List<string> excludedList = new List<string>();

		void OnEnable()
		{
			if (treeviewState == null)
				treeviewState = new TreeViewState();

			myTreeView = new MyTreeView(treeviewState);
		}

		public static int id;

		[MenuItem("TutoTOONS/Optimise Images For ETC2")]
		public static void ShowWindow()
		{
			var window = GetWindow(typeof(BuildSizeOptimizer));
			window.titleContent = new GUIContent("Optimise Images For ETC2");
			window.Show();
		}

		public static void GetDirs(string path, TreeViewItem tvi)
		{
			var pth = path.Substring(Application.dataPath.Length + 1);
			if (excludedList.Contains(pth)) return;
			var newTvi = new TreeViewItem { id = id++, displayName = pth };
			tvi.AddChild(newTvi);
			var dirs = Directory.GetDirectories(path);

			for (int i = 0; i < dirs.Length; i++)
			{
				GetDirs(dirs[i], newTvi);
			}
		}

		static TreeViewItem FindItemRecursive(int id, TreeViewItem item)
		{
			if (item == null)
				return null;

			if (item.id == id)
				return item;

			if (!item.hasChildren)
				return null;

			foreach (TreeViewItem child in item.children)
			{
				TreeViewItem result = FindItemRecursive(id, child);
				if (result != null)
					return result;
			}
			return null;
		}

		public static void AddDirs(string path)
		{
			if (excludedList.Contains(path)) return;

			excludedList.Add(path);
			var dirs = Directory.GetDirectories(Application.dataPath + "/" + path);

			for (int i = 0; i < dirs.Length; i++)
			{
				AddDirs(dirs[i].Substring(Application.dataPath.Length + 1));
			}
		}

		void OnGUI()
		{
			limit1024 = GUILayout.Toggle(limit1024, "Limit to 1024x1024");
			forceNormalCompression = GUILayout.Toggle(forceNormalCompression, "Force to Normal Compression");

			if (GUILayout.Button("Remove Selected"))
			{
				var selection = myTreeView.state.selectedIDs;

				for (int i = 0; i < selection.Count; i++)
				{
					AddDirs(FindItemRecursive(selection[i], myTreeView.root).displayName);
				}
				myTreeView.Reload();
			}
			if (GUILayout.Button("Reset"))
			{
				excludedList.Clear();
				myTreeView.Reload();
			}
			if (GUILayout.Button("ExpandTexturesForETC2"))
			{
				ExpandTexturesForETC2(limit1024, forceNormalCompression);
			}
			myTreeView.OnGUI(new Rect(0, 128, position.width, position.height));
		}

		static int MultipleOfFour(float value)
		{
			return Mathf.CeilToInt(value / 4f) * 4;
		}
		
		public static void ExpandTexturesForETC2(bool limitSize, bool forceCompression)
		{
			if (EditorUtility.DisplayDialog("Confirm", "Optimize all textures? (Can be canceled)", "Yes", "No"))
			{
				Debug.Log("Started optimizing textures");

				var guids = AssetDatabase.FindAssets("t:Texture");

				var len = guids.Length;
				for (int i = 0; i < len; i++)
				{
					try
					{
						var guid = guids[i];
						var path = AssetDatabase.GUIDToAssetPath(guid);

						if (excludedList.Contains(Path.GetDirectoryName(path.Substring(7)))) //Assets/
						{
							continue;
						}
						var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

						var testWidth = MultipleOfFour(asset.width);
						var testHeight = MultipleOfFour(asset.height);


						if (EditorUtility.DisplayCancelableProgressBar("Progress", i + "/" + len + " | " + path, i / (float)len)) break;

						var importer = AssetImporter.GetAtPath(path) as TextureImporter;

						if (importer == null) continue;

						var prevReadable = importer.isReadable;

						importer.isReadable = true;

						var alreadyMultipleOfFour = asset.width == testWidth && asset.height == testHeight;
						var sizeShouldBeLimited = limitSize && importer.maxTextureSize > 1024;
						var compressionShouldBeForced = forceCompression && importer.textureCompression != TextureImporterCompression.Compressed;

						if (alreadyMultipleOfFour && !sizeShouldBeLimited && !compressionShouldBeForced) continue;

						object[] args = new object[2] { 0, 0 };
						MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
						mi.Invoke(importer, args);

						var originalWidth = (int)args[0];
						var originalHeight = (int)args[1];
						
						var resizedWidth = MultipleOfFour(originalWidth);
						var resizedHeight = MultipleOfFour(originalHeight);

						var maxTexSize = limitSize ? Mathf.Min(importer.maxTextureSize, 1024) : importer.maxTextureSize;

						if (resizedWidth > resizedHeight)
						{
							if (resizedWidth > maxTexSize)
							{
								var multiplier = maxTexSize / (float)resizedWidth;
								var hDown = MultipleOfFour(resizedHeight * multiplier);
								resizedHeight = Mathf.RoundToInt(hDown / multiplier);
							}
						}
						else
						{
							if (resizedHeight > maxTexSize)
							{
								var multiplier = maxTexSize / (float)resizedHeight;
								var wDown = MultipleOfFour(resizedWidth * multiplier);
								resizedWidth = Mathf.RoundToInt(wDown / multiplier);
							}
						}
						
						var prevMaxSize = importer.maxTextureSize;
						var prevCrunch = importer.crunchedCompression;
						importer.crunchedCompression = false;
						importer.maxTextureSize = 8192;
						importer.SaveAndReimport();
						
						var newTex = new Texture2D(resizedWidth, resizedHeight, TextureFormat.ARGB32, false);

						var assetPixels = asset.GetPixels();
						var pixels = new Color[resizedWidth * resizedHeight];

						var scaleRatioW = (float)originalWidth / resizedWidth;
						var scaleRatioH = (float)originalHeight / resizedHeight;

						//New upscaling
						for (int y = 0; y < resizedHeight; y++)
						{
							for (int x = 0; x < resizedWidth; x++)
							{
								var x1 = Mathf.FloorToInt(x * scaleRatioW);
								var y1 = Mathf.FloorToInt(y * scaleRatioH);
								
								var pixel = assetPixels[Mathf.Min(y1 * asset.width + x1, assetPixels.Length - 1)];
								
								pixels[y * resizedWidth + x] = pixel;
							}
						}

						//Older pixel gaps solution
						/*for (int y = 0; y < resizedHeight; y++)
						{
							for (int x = 0; x < resizedWidth; x++)
							{
								if (x >= asset.width || y >= asset.height)
								{
									pixels[y * resizedWidth + x] = Color.clear;
									continue;
								}
								pixels[y * resizedWidth + x] = assetPixels[y * asset.width + x];
							}
						}*/

						newTex.SetPixels(pixels);
						newTex.Apply();
						var bytes = newTex.EncodeToPNG();
						var filePath = Application.dataPath + path.Substring(6, path.Length - 6);
						File.WriteAllBytes(filePath, bytes);

						importer.isReadable = prevReadable;
						importer.crunchedCompression = prevCrunch;
						importer.maxTextureSize = maxTexSize;
						if (forceNormalCompression)
						{
							importer.textureCompression = TextureImporterCompression.Compressed;
						}
						importer.SaveAndReimport();

						EditorUtility.SetDirty(asset);
					}
					catch (System.Exception e)
					{
						Debug.Log("Error: " + e.Message + ":" + e.StackTrace);
					}
				}
				Debug.Log("Optimizing textures completed");
				EditorUtility.ClearProgressBar();
				AssetDatabase.SaveAssets();
			}
		}
	}
}
using UnityEngine;
using UnityEditor;

namespace SubjectNerd.FMod.Editor
{
	[CustomEditor(typeof(FMODMaster))]
	public class FMODMasterInspector : UnityEditor.Editor
	{
		private FMODMaster master;
		void OnEnable()
		{
			master = target as FMODMaster;
		}

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			DrawVolume("Master", master.Mixers[0]);
			for (int i = 1; i < master.Mixers.Length; i++)
			{
				var mixerData = master.Mixers[i];
				var name = mixerData.path.Substring(5);
				DrawVolume(name, mixerData);
			}
		}

		private void DrawVolume(string label, FMODMaster.MixerData data)
		{
			float newVolume = EditorGUILayout.Slider(label, data.Volume, 0f, 1f);
			if (GUI.changed)
				data.Volume = newVolume;
		}
	}
}
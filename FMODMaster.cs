using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;

namespace SubjectNerd.FMod
{
	public class FMODMaster : MonoBehaviour
	{
		#region Static
		private static FMODMaster _instance;

		/// <summary>
		/// An instance of FMOD Master
		/// </summary>
		public static FMODMaster Instance
		{
			get
			{
				Initialize();
				return _instance;
			}
		}

		/// <summary>
		/// Creates an instance of FMOD Master
		/// </summary>
		/// <returns>True if first initialization</returns>
		public static bool Initialize()
		{
			if (_instance != null)
				return false;

			// Spawn the system FMOD system first
			_system = FMOD_StudioSystem.instance;

			// Then spawn FMOD Master
			GameObject go = new GameObject("FMOD_Master");
			_instance = go.AddComponent<FMODMaster>();

			// And put sytem under master
			_system.transform.parent = go.transform;
			return true;
		}

		private static FMOD_StudioSystem _system;

		public static bool ERRCHECK(FMOD.RESULT result)
		{
			bool pass = FMOD.Studio.UnityUtil.ERRCHECK(result);
			if (!pass)
				Debug.LogError("FMOD error: " + result);
			return pass;
		}
		#endregion

		public class MixerData
		{
			private float _volume;
			private float _startVolume;

			public string path;
			public MixerStrip mixer;

			public MixerData(string path, MixerStrip mixer)
			{
				this.path = path;
				this.mixer = mixer;

				_volume = 1f;
				ERRCHECK(mixer.getFaderLevel(out _startVolume));
			}

			public float Volume
			{
				get { return _volume; }
				set
				{
					_volume = value;
					float volumeValue = _startVolume*_volume;
					ERRCHECK(mixer.setFaderLevel(volumeValue));
				}
			}
		}

		private MixerStrip mixerMaster;
		private List<MixerData> mixers = new List<MixerData>();

		public MixerData[] Mixers
		{
			get { return mixers.ToArray(); }
		} 

		// Use this for initialization
		void Start ()
		{
			FetchMixer("bus:/"); // First fetch the master mixer to slot 0
			// Load the fmod config to get volume channels
			FMODConfig conf = Resources.Load<FMODConfig>("Config/FMODConfig");
			if (conf==null) // No config loaded, do nothing
				return;
		
			// Fetch the rest of the mixers defined in the config
			foreach (var busPath in conf.soundMixers)
			{
				FetchMixer(busPath);
			}
		}
	
		void FetchMixer(string mixerPath)
		{
			FMOD.GUID guid;
			MixerStrip mixer;
			ERRCHECK(_system.System.lookupID(mixerPath, out guid));
			if (ERRCHECK(_system.System.getMixerStrip(guid, LOADING_MODE.BEGIN_NOW, out mixer)))
			{
				float initialVolume;
				if (ERRCHECK(mixer.getFaderLevel(out initialVolume)))
				{
					mixers.Add(new MixerData(mixerPath, mixer));
				}
			}
		}

		/// <summary>
		/// Play a FMOD sound asset. No stop control.
		/// </summary>
		/// <param name="asset">FMOD Asset to play</param>
		/// <param name="position">Position to play the sound from</param>
		public void PlayOneShot(FMODAsset asset, Vector3 position)
		{
			_system.PlayOneShot(asset.id, position);
		}

		/// <summary>
		/// Play a FMOD sound event. No stop control.
		/// </summary>
		/// <param name="path">FMOD Event path</param>
		/// <param name="position">Position to play the sound from</param>
		public void PlayOneShot(string path, Vector3 position)
		{
			_system.PlayOneShot(path, position);
		}

		/// <summary>
		/// Play a FMOD sound asset, user is responsible for stopping and disposal.
		/// </summary>
		/// <param name="asset">FMOD Asset to play</param>
		/// <param name="position">Position to start sound from</param>
		/// <returns>EventInstance for the sound</returns>
		public EventInstance PlayAsset(FMODAsset asset, Vector3 position)
		{
			return PlayAsset(asset.id, position, 1);
		}

		/// <summary>
		/// Play a FMOD sound asset, user is responsible for stopping and disposal.
		/// </summary>
		/// <param name="path">FMOD Event path</param>
		/// <param name="position">Position to start sound from</param>
		/// <returns>EventInstance for the sound</returns>
		public EventInstance PlayAsset(string path, Vector3 position)
		{
			return PlayAsset(path, position, 1);
		}

		private EventInstance PlayAsset(string path, Vector3 position, float volume)
		{
			// Basically a copy of FMOD_StudioSystem.PlayOneShot, but returns the event instance
			EventInstance instance = _system.GetEvent(path);
			var attributes = FMOD.Studio.UnityUtil.to3DAttributes(position);
			ERRCHECK(instance.set3DAttributes(attributes));
			ERRCHECK(instance.start());
			return instance;
		}

		/// <summary>
		/// Stops and disposes an FMOD EventInstance
		/// </summary>
		/// <param name="instance">Instance to stop</param>
		/// <param name="immediate">Stop immediately or allow to fade out</param>
		/// <returns></returns>
		public bool StopInstance(ref EventInstance instance, bool immediate = true)
		{
			ERRCHECK(instance.stop(immediate ? STOP_MODE.IMMEDIATE : STOP_MODE.ALLOWFADEOUT));
			bool didRelease = ERRCHECK(instance.release());
			if (didRelease)
				instance = null;
			return didRelease;
		}
	}
}
"""Generate the dedicated mixer using stable subasset IDs. Run outside Play Mode, then run Lighthouse Keepers/Audio/Install master ceiling."""
from pathlib import Path
import uuid
root=Path(__file__).resolve().parents[2];out=root/'Assets/WatchDemo/Resources/KeeperAudio.mixer'
names=['Master','VoiceChat','Radio','SFX_Critical','SFX_World','Ambience','Music'];vols=[0,0,0,-3,-6,-4,-2]
def guid(s):return uuid.uuid5(uuid.NAMESPACE_URL,'lighthousekeepers/audio-pass/'+s).hex
s='%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n'
def obj(kind,id,body):
 global s
 s+=f'--- !u!{kind} &{id}\n'+body
base='  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n'
obj(241,24100000,'AudioMixerController:\n'+base+'  m_Name: KeeperAudio\n  m_OutputGroup: {fileID: 0}\n  m_MasterGroup: {fileID: 24300010}\n  m_Snapshots:\n  - {fileID: 24500006}\n  m_StartSnapshot: {fileID: 24500006}\n  m_SuspendThreshold: -80\n  m_EnableSuspend: 1\n  m_UpdateMode: 0\n  m_ExposedParameters:\n'+''.join(f'  - guid: {guid(n+"volume")}\n    name: {n}Volume\n' for n in names[1:])+'  m_AudioMixerGroupViews: []\n  m_CurrentViewIndex: 0\n  m_TargetSnapshot: {fileID: 24500006}\n')
for i,n in enumerate(names):
 children='\n'+''.join(f'  - {{fileID: {24300010+j}}}\n' for j in range(1,len(names))) if i==0 else ' []\n'
 obj(243,24300010+i,'AudioMixerGroupController:\n'+base+f'  m_Name: {n}\n  m_AudioMixer: {{fileID: 24100000}}\n  m_GroupID: {guid(n)}\n  m_Children:'+children+f'  m_Volume: {guid(n+"volume")}\n  m_Pitch: {guid(n+"pitch")}\n  m_Send: 00000000000000000000000000000000\n  m_Effects:\n  - {{fileID: {24400010+i}}}\n  m_UserColorIndex: 0\n  m_Mute: 0\n  m_Solo: 0\n  m_BypassEffects: 0\n')
 obj(244,24400010+i,'AudioMixerEffectController:\n'+base.replace('m_ObjectHideFlags: 0','m_ObjectHideFlags: 3')+f'  m_Name: \n  m_EffectID: {guid(n+"effect")}\n  m_EffectName: Attenuation\n  m_MixLevel: {guid(n+"mix")}\n  m_Parameters: []\n  m_SendTarget: {{fileID: 0}}\n  m_EnableWetMix: 0\n  m_Bypass: 0\n')
obj(245,24500006,'AudioMixerSnapshotController:\n'+base+f'  m_Name: Default\n  m_AudioMixer: {{fileID: 24100000}}\n  m_SnapshotID: {guid("snapshot")}\n  m_FloatValues:\n'+''.join(f'    {guid(n+"volume")}: {v}\n    {guid(n+"pitch")}: 1\n' for n,v in zip(names,vols))+'  m_TransitionOverrides: {}\n')
out.write_text(s)

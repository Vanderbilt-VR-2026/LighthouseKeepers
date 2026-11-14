using System;
namespace LighthouseKeepers.WatchDemo {
 public sealed partial class WatchState {
  public bool Tutorial=>Campaign?Scenario.noFail:Master&&Master.onboarding&&Day==1;
  public bool GeneratorTutorial=>!Campaign&&Master&&Master.onboarding&&Day==2&&Engine!=null;
  public bool NightTwoOutage {get;private set;}
  double firstGeneratorGraceUntil,simulationSliceEnd;
  public bool NightTwoFuelFailure=>GeneratorTutorial&&Master.nightTwoFuelFailure||CampaignFuelFault!=null;
  public float TutorialFuelDrain=>CampaignFuelFault!=null?Scenario.arrivalFuelFraction/UnityEngine.Mathf.Max(1,CampaignFuelFault.atSeconds):Definition.initialFuelFraction/UnityEngine.Mathf.Max(1,Master.nightTwoOutageSeconds);
  public double FuelRunoutRemaining=>NightTwoFuelFailure&&!NightTwoOutage&&Engine.Running?Engine.Fuel/UnityEngine.Mathf.Max(.00001f,Engine.Consumption):double.PositiveInfinity;
  public bool FuelSputtering=>NightTwoFuelFailure&&FuelRunoutRemaining<=Master.fuelSputterSeconds;
  void ObserveTutorialFuelStop(){if(!NightTwoFuelFailure||NightTwoOutage||Engine.Running)return;NightTwoOutage=true;firstGeneratorGraceUntil=Math.Max(Elapsed,simulationSliceEnd)+Master.firstGeneratorGraceSeconds;Engine.SetConsumption(Definition.fuelDrainPerSecond);}

  void TutorialOutage(){if(ReadOnly||!GeneratorTutorial||NightTwoOutage)return;NightTwoOutage=true;firstGeneratorGraceUntil=Elapsed+Master.firstGeneratorGraceSeconds;Engine.Reset(Definition.initialFuelFraction,Definition.fuelDrainPerSecond,false);Changed?.Invoke();}
 }
}
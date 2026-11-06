using System;
namespace LighthouseKeepers.WatchDemo {
 public sealed partial class WatchState {
  public bool Tutorial=>Master&&Master.onboarding&&Day==1;
  public bool GeneratorTutorial=>Master&&Master.onboarding&&Day==2&&Engine!=null;
  public bool NightTwoOutage {get;private set;}
  double firstGeneratorGraceUntil;
  void TutorialOutage(){if(ReadOnly||!GeneratorTutorial||NightTwoOutage)return;NightTwoOutage=true;firstGeneratorGraceUntil=Elapsed+Master.firstGeneratorGraceSeconds;Engine.Reset(Definition.initialFuelFraction,Definition.fuelDrainPerSecond,false);Changed?.Invoke();}
 }
}
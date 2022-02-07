namespace TutoTOONS
{
    public class TutoToolsEditor : TutoToonsEditor
    {
        public override void RunPostProcessTasksAndroid()
        {
            AddGradleDependeny("implementation 'com.google.android.gms:play-services-appset:16.0.0'");
        }
    }
}
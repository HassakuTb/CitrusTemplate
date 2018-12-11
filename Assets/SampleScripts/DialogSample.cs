using UnityEngine;

namespace Citrus.UI.Sample {
    public class DialogSample : MonoBehaviour {

        [SerializeField] private Dialog prefab;
        

        public void OpenDialog()
        {
            DialogBuilder builder = new DialogBuilder(prefab);
            builder.Message("This is Dialog Message.\nYou can write multi line text.");
            builder.CenterButton("Log", _ => { Debug.Log("Log Button Pressed"); });
            builder.RightButton("Close", d => { d.Hide(destroy:true); });

            Dialog dialog = builder.Build();
            dialog.Show();
        }
    }

}
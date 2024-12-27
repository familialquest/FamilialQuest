using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public sealed class UIElement : UIBehaviour {

 public Animator aniamtor { get; private set; }
 public bool isOpen { get; private set; }

 protected override void Start() {
  base.Start();
  this.aniamtor = GetComponent<Animator>();
 }

 public void OnClick() {
  this.isOpen = !this.isOpen;
  this.aniamtor.SetBool("IsOpen", this.isOpen);
 }

}
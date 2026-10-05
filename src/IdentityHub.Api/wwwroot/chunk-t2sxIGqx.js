import{$ as Kh,A as Et$1,B as He,Bn as kP,Cn as fr,Cr as w,Ct as Rl,Dn as gp,Dt as SE,E as Dp,En as gl,Er as xE,Et as S,F as G,Fr as yu,Ft as UE,Jn as me,Jt as Y,K as Hp,Lt as Uh,N as Fe,Nn as iE,Or as xa,Pn as iu,Pr as yr,Pt as U,Q as Jh,Sr as vr,Tn as ge,Tr as wp,Tt as Rp,U as Hh,Un as lE,Vn as kh,Vt as Vh,Wn as lp,Xn as mp,Y as Ip,_ as CE,_r as tr,_t as Op,at as Mc,b as Cp,bt as Po,ct as Mp,dn as ce,en as _E,et as Kt,fr as tI,ft as OE,gn as eD,h as Bp,in as _u,it as ME,j as FI,jn as hp,jt as TD,k as Ep,kr as xc,kt as Sc,lr as sE,lt as Nc,mn as dD,n as $I,nn as _p,p as Bi$1,pn as cp,qn as mE,qt as Xc,rr as pi,rt as M,sn as av,st as Mi,t as $,tn as _e,tr as ou,ur as sp,vr as uD,vt as PP,wr as wm,xn as fD,xt as Py,yn as ei$1,yr as uE,zt as VI}from"./chunk-BAGO138l.js";import"./chunk-BRU6aIvc.js";import{a as ce$1,c as r,i as bo,o as ur,r as Tt,t as m}from"./main-EKR7QEBK.js";import{a as ne,i as gt,o as oe,r as ct,s as st}from"./chunk-CFOwrj46.js";import{A as gt$1,B as ri,C as b,D as fn$1,E as ee,F as li,G as yn$1,I as lt,J as zn$1,K as ys,L as oi,O as ft,R as pt,S as as,T as di,U as xt,V as ts,W as ye$1,_ as Ta,a as En$1,b as Yo,c as Gn$1,d as Jt,f as Ks,i as Ei,j as ii$1,k as ge$1,m as Me$1,n as Ce$1,o as Ft,p as Ln$1,q as yt,r as De$1,s as Gi$1,t as Bt,u as Ji$1,v as Ui,w as ct$1,x as Zt,y as Xs,z as qn$1}from"./chunk-DBOXxbVB.js";import{n as mt$1,t as Yt}from"./chunk-C7T5svkV.js";import"./chunk-NBTsMKfk.js";import{t as l}from"./chunk-BYthlU3b.js";import{t as p}from"./chunk-Wdie0Eb2.js";import{t as h}from"./chunk-hS4H3W05.js";import{n as G$1,t as F}from"./chunk-BlSFlIKh.js";var en=[`*`,[[`mat-toolbar-row`]]];var nn=[`*`,`mat-toolbar-row`];var an=(()=>{class n{static ɵfac=function(e){return new(e||n)};static ɵdir=$I({type:n,selectors:[[`mat-toolbar-row`]],hostAttrs:[1,`mat-toolbar-row`],exportAs:[`matToolbarRow`]})}return n})();var fe=(()=>{class n{_elementRef=w(vr);_platform=w(b);_document=w(tr);color;_toolbarRows;ngAfterViewInit(){this._platform.isBrowser&&(this._checkToolbarMixedModes(),this._toolbarRows.changes.subscribe(()=>this._checkToolbarMixedModes()))}_checkToolbarMixedModes(){this._toolbarRows.length}static ɵfac=function(e){return new(e||n)};static ɵcmp=FI({type:n,selectors:[[`mat-toolbar`]],contentQueries:function(e,i,a){if(e&1&&_p(a,an,5),e&2){let r;SE(r=xE())&&(i._toolbarRows=r)}},hostAttrs:[1,`mat-toolbar`],hostVars:6,hostBindings:function(e,i){e&2&&(UE(i.color?`mat-`+i.color:``),Rp(`mat-toolbar-multiple-rows`,i._toolbarRows.length>0)(`mat-toolbar-single-row`,i._toolbarRows.length===0))},inputs:{color:`color`},exportAs:[`matToolbar`],ngContentSelectors:nn,decls:2,vars:0,template:function(e,i){e&1&&(_E(en),ME(0),ME(1,1))},styles:[`.mat-toolbar {
  background: var(--%NS%mat-toolbar-container-background-color, var(--%NS%mat-sys-surface));
  color: var(--%NS%mat-toolbar-container-text-color, var(--%NS%mat-sys-on-surface));
}
.mat-toolbar, .mat-toolbar h1, .mat-toolbar h2, .mat-toolbar h3, .mat-toolbar h4, .mat-toolbar h5, .mat-toolbar h6 {
  font-family: var(--%NS%mat-toolbar-title-text-font, var(--%NS%mat-sys-title-large-font));
  font-size: var(--%NS%mat-toolbar-title-text-size, var(--%NS%mat-sys-title-large-size));
  line-height: var(--%NS%mat-toolbar-title-text-line-height, var(--%NS%mat-sys-title-large-line-height));
  font-weight: var(--%NS%mat-toolbar-title-text-weight, var(--%NS%mat-sys-title-large-weight));
  letter-spacing: var(--%NS%mat-toolbar-title-text-tracking, var(--%NS%mat-sys-title-large-tracking));
  margin: 0;
}
@media (forced-colors: active) {
  .mat-toolbar {
    outline: solid 1px;
  }
}
.mat-toolbar .mat-form-field-underline,
.mat-toolbar .mat-form-field-ripple,
.mat-toolbar .mat-focused .mat-form-field-ripple {
  background-color: currentColor;
}
.mat-toolbar .mat-form-field-label,
.mat-toolbar .mat-focused .mat-form-field-label,
.mat-toolbar .mat-select-value,
.mat-toolbar .mat-select-arrow,
.mat-toolbar .mat-form-field.mat-focused .mat-select-arrow {
  color: inherit;
}
.mat-toolbar .mat-input-element {
  caret-color: currentColor;
}
.mat-toolbar .mat-mdc-button-base.mat-mdc-button-base.mat-unthemed {
  --%NS%mat-button-text-label-text-color: var(--%NS%mat-toolbar-container-text-color, var(--%NS%mat-sys-on-surface));
  --%NS%mat-button-outlined-label-text-color: var(--%NS%mat-toolbar-container-text-color, var(--%NS%mat-sys-on-surface));
}

.mat-toolbar-row, .mat-toolbar-single-row {
  display: flex;
  box-sizing: border-box;
  padding: 0 16px;
  width: 100%;
  flex-direction: row;
  align-items: center;
  white-space: nowrap;
  height: var(--%NS%mat-toolbar-standard-height, 64px);
}
@media (max-width: 599px) {
  .mat-toolbar-row, .mat-toolbar-single-row {
    height: var(--%NS%mat-toolbar-mobile-height, 56px);
  }
}

.mat-toolbar-multiple-rows {
  display: flex;
  box-sizing: border-box;
  flex-direction: column;
  width: 100%;
  min-height: var(--%NS%mat-toolbar-standard-height, 64px);
}
@media (max-width: 599px) {
  .mat-toolbar-multiple-rows {
    min-height: var(--%NS%mat-toolbar-mobile-height, 56px);
  }
}
`],encapsulation:2})}return n})();var be=(()=>{class n{static ɵfac=function(e){return new(e||n)};static ɵmod=VI({type:n});static ɵinj=Rl({imports:[st]})}return n})();var ye=[`*`];var on=[`content`];var Bi=[[[`mat-drawer`],[`mat-sidenav`]],[[`mat-drawer-content`],[`mat-sidenav-content`]],`*`];var zi=[`mat-drawer, mat-sidenav`,`mat-drawer-content, mat-sidenav-content`,`*`];function rn(n,s){if(n&1){let t=mE();pi(0,`div`,1),wp(`click`,function(){ou(t);return iu(CE()._onBackdropClicked())}),Mc()}if(n&2)Rp(`mat-drawer-shown`,CE()._isShowingBackdrop())}function sn(n,s){n&1&&(pi(0,`mat-drawer-content`),ME(1,2),Mc())}function ln(n,s){if(n&1){let t=mE();pi(0,`div`,1),wp(`click`,function(){ou(t);return iu(CE()._onBackdropClicked())}),Mc()}if(n&2)Rp(`mat-drawer-shown`,CE()._isShowingBackdrop())}function cn(n,s){n&1&&(pi(0,`mat-sidenav-content`),ME(1,2),Mc())}var mn=`.mat-drawer-container {
  position: relative;
  z-index: 1;
  color: var(--%NS%mat-sidenav-content-text-color, var(--%NS%mat-sys-on-background));
  background-color: var(--%NS%mat-sidenav-content-background-color, var(--%NS%mat-sys-background));
  box-sizing: border-box;
  display: block;
  overflow: hidden;
}
.mat-drawer-container[fullscreen] {
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  position: absolute;
}
.mat-drawer-container[fullscreen].mat-drawer-container-has-open {
  overflow: hidden;
}
.mat-drawer-container.mat-drawer-container-explicit-backdrop .mat-drawer-side {
  z-index: 3;
}
.mat-drawer-container.ng-animate-disabled .mat-drawer-backdrop,
.mat-drawer-container.ng-animate-disabled .mat-drawer-content, .ng-animate-disabled .mat-drawer-container .mat-drawer-backdrop,
.ng-animate-disabled .mat-drawer-container .mat-drawer-content {
  transition: none;
}

.mat-drawer-backdrop {
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  position: absolute;
  display: block;
  z-index: 3;
  visibility: hidden;
}
.mat-drawer-backdrop.mat-drawer-shown {
  visibility: visible;
  background-color: var(--%NS%mat-sidenav-scrim-color, color-mix(in srgb, var(--%NS%mat-sys-neutral-variant20) 40%, transparent));
}
.mat-drawer-transition .mat-drawer-backdrop {
  transition-duration: 400ms;
  transition-timing-function: cubic-bezier(0.25, 0.8, 0.25, 1);
  transition-property: background-color, visibility;
}
@media (forced-colors: active) {
  .mat-drawer-backdrop {
    opacity: 0.5;
  }
}

.mat-drawer-content {
  position: relative;
  z-index: 1;
  display: block;
  height: 100%;
  overflow: auto;
}
.mat-drawer-content.mat-drawer-content-hidden {
  opacity: 0;
}
.mat-drawer-transition .mat-drawer-content {
  transition-duration: 400ms;
  transition-timing-function: cubic-bezier(0.25, 0.8, 0.25, 1);
  transition-property: transform, margin-left, margin-right;
}

.mat-drawer {
  position: relative;
  z-index: 4;
  color: var(--%NS%mat-sidenav-container-text-color, var(--%NS%mat-sys-on-surface-variant));
  box-shadow: var(--%NS%mat-sidenav-container-elevation-shadow, none);
  background-color: var(--%NS%mat-sidenav-container-background-color, var(--%NS%mat-sys-surface));
  border-top-right-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-bottom-right-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  width: var(--%NS%mat-sidenav-container-width, 360px);
  display: block;
  position: absolute;
  top: 0;
  bottom: 0;
  z-index: 3;
  outline: 0;
  box-sizing: border-box;
  overflow-y: auto;
  transform: translate3d(-100%, 0, 0);
}
@media (forced-colors: active) {
  .mat-drawer, [dir=rtl] .mat-drawer.mat-drawer-end {
    border-right: solid 1px currentColor;
  }
}
@media (forced-colors: active) {
  [dir=rtl] .mat-drawer, .mat-drawer.mat-drawer-end {
    border-left: solid 1px currentColor;
    border-right: none;
  }
}
.mat-drawer.mat-drawer-side {
  z-index: 2;
}
.mat-drawer.mat-drawer-end {
  right: 0;
  transform: translate3d(100%, 0, 0);
  border-top-left-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-bottom-left-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-top-right-radius: 0;
  border-bottom-right-radius: 0;
}
[dir=rtl] .mat-drawer {
  border-top-left-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-bottom-left-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-top-right-radius: 0;
  border-bottom-right-radius: 0;
  transform: translate3d(100%, 0, 0);
}
[dir=rtl] .mat-drawer.mat-drawer-end {
  border-top-right-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-bottom-right-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-top-left-radius: 0;
  border-bottom-left-radius: 0;
  left: 0;
  right: auto;
  transform: translate3d(-100%, 0, 0);
}
.mat-drawer-transition .mat-drawer {
  transition: transform 400ms cubic-bezier(0.25, 0.8, 0.25, 1);
}
.mat-drawer:not(.mat-drawer-opened):not(.mat-drawer-animating) {
  visibility: hidden;
  box-shadow: none;
}
.mat-drawer:not(.mat-drawer-opened):not(.mat-drawer-animating) .mat-drawer-inner-container {
  display: none;
}
.mat-drawer.mat-drawer-opened.mat-drawer-opened {
  transform: none;
}

.mat-drawer-side {
  box-shadow: none;
  border-right-color: var(--%NS%mat-sidenav-container-divider-color, transparent);
  border-right-width: 1px;
  border-right-style: solid;
}
.mat-drawer-side.mat-drawer-end {
  border-left-color: var(--%NS%mat-sidenav-container-divider-color, transparent);
  border-left-width: 1px;
  border-left-style: solid;
  border-right: none;
}
[dir=rtl] .mat-drawer-side {
  border-left-color: var(--%NS%mat-sidenav-container-divider-color, transparent);
  border-left-width: 1px;
  border-left-style: solid;
  border-right: none;
}
[dir=rtl] .mat-drawer-side.mat-drawer-end {
  border-right-color: var(--%NS%mat-sidenav-container-divider-color, transparent);
  border-right-width: 1px;
  border-right-style: solid;
  border-left: none;
}

.mat-drawer-inner-container {
  width: 100%;
  height: 100%;
  overflow: auto;
}

.mat-sidenav-fixed {
  position: fixed;
}
`;var dn=new S(`MAT_DRAWER_DEFAULT_AUTOSIZE`,{providedIn:`root`,factory:()=>!1});var Ke=new S(`MAT_DRAWER_CONTAINER`);var Vt=(()=>{class n extends Gi$1{_platform=w(b);_changeDetectorRef=w(kP);_element=w(vr);_ngZone=w(_e);_isInert=!1;_container=w(Ye);ngAfterContentInit(){this._container._contentMarginChanges.subscribe(()=>this._changeDetectorRef.markForCheck())}_drawerToggled(t){t.opened?this._ngZone.runOutsideAngular(()=>{t._animationEnd.pipe(Uh(50),Et$1(1)).subscribe(()=>this._updateInert())}):this._updateInert()}_drawerModeChanged(){this._updateInert()}_updateInert(){let t=this._container._isShowingBackdrop();if(t!==this._isInert){let e=this._element.nativeElement;this._isInert=t,t?e.setAttribute(`inert`,`true`):e.removeAttribute(`inert`)}}_shouldBeHidden(){if(this._platform.isBrowser)return!1;let{start:t,end:e}=this._container;return t!=null&&t.mode!==`over`&&t.opened||e!=null&&e.mode!==`over`&&e.opened}static ɵfac=(()=>{let t;return function(i){return(t||(t=wm(n)))(i||n)}})();static ɵcmp=FI({type:n,selectors:[[`mat-drawer-content`]],hostAttrs:[1,`mat-drawer-content`],hostVars:6,hostBindings:function(e,i){e&2&&(Op(`margin-left`,i._container._contentMargins.left,`px`)(`margin-right`,i._container._contentMargins.right,`px`),Rp(`mat-drawer-content-hidden`,i._shouldBeHidden()))},features:[uD([{provide:Gi$1,useExisting:n}]),sp],ngContentSelectors:ye,decls:1,vars:0,template:function(e,i){e&1&&(_E(),ME(0))},encapsulation:2})}return n})();var qe=(()=>{class n{_elementRef=w(vr);_focusTrapFactory=w(Ei);_focusMonitor=w(Bt);_platform=w(b);_ngZone=w(_e);_renderer=w(xa);_interactivityChecker=w(yn$1);_doc=w(tr);_isAnimating=!1;_container=w(Ke,{optional:!0});_focusTrap=null;_elementFocusedBeforeDrawerWasOpened=null;_eventCleanups;_isAttached=!1;_anchor=null;get position(){return this._position}set position(t){t=t===`end`?`end`:`start`,t!==this._position&&(this._isAttached&&this._updatePositionInParent(t),this._position=t,this.onPositionChanged.emit())}_position=`start`;get mode(){return this._mode}set mode(t){this._mode=t,this._updateFocusTrapState(),this._modeChanged.next(),this._getContent()?._drawerModeChanged()}_mode=`over`;get disableClose(){return this._disableClose}set disableClose(t){this._disableClose=as(t)}_disableClose=!1;get autoFocus(){return this._autoFocus??(this.mode===`side`?`dialog`:`first-tabbable`)}set autoFocus(t){(t===`true`||t===`false`||t==null)&&(t=as(t)),this._autoFocus=t}_autoFocus;get opened(){return this._opened()}set opened(t){this.toggle(as(t))}_opened=Po(!1);_openedVia=null;_animationStarted=new Y;_animationEnd=new Y;openedChange=new He(!0);_openedStream=this.openedChange.pipe(Kt(t=>t),Fe(()=>{}));openedStart=this._animationStarted.pipe(Kt(()=>this.opened),Bi$1(void 0));_closedStream=this.openedChange.pipe(Kt(t=>!t),Fe(()=>{}));closedStart=this._animationStarted.pipe(Kt(()=>!this.opened),Bi$1(void 0));_destroyed=new Y;onPositionChanged=new He;_content;_modeChanged=new Y;_injector=w(ge);_changeDetectorRef=w(kP);constructor(){this.openedChange.pipe(Jh(this._destroyed)).subscribe(t=>{t?(this._elementFocusedBeforeDrawerWasOpened=this._doc.activeElement,this._takeFocus()):this._isFocusWithinDrawer()&&this._restoreFocus(this._openedVia||`program`)}),this._eventCleanups=this._ngZone.runOutsideAngular(()=>{let t=this._renderer,e=this._elementRef.nativeElement;return[t.listen(e,`keydown`,i=>{i.keyCode===27&&!this.disableClose&&!En$1(i)&&this._ngZone.run(()=>{this.close(),i.stopPropagation(),i.preventDefault()})}),t.listen(e,`transitionend`,this._handleTransitionEvent),t.listen(e,`transitioncancel`,this._handleTransitionEvent)]}),this._animationEnd.subscribe(()=>{this.openedChange.emit(this.opened)})}_focusByCssSelector(t,e){let i=this._elementRef.nativeElement.querySelector(t);i&&(this._interactivityChecker.isFocusable(i)||(i.tabIndex=-1,this._ngZone.runOutsideAngular(()=>{let a=()=>{r(),d(),i.removeAttribute(`tabindex`)},r=this._renderer.listen(i,`blur`,a),d=this._renderer.listen(i,`mousedown`,a)})),i.focus(e))}_takeFocus(){if(!this._focusTrap)return;let t=this._elementRef.nativeElement;switch(this.autoFocus){case!1:case`dialog`:return;case!0:case`first-tabbable`:Py(()=>{let e=this._isAnimating?{preventScroll:!0}:void 0;!this._focusTrap.focusInitialElement(e)&&typeof t.focus==`function`&&t.focus(e)},{injector:this._injector});break;case`first-heading`:this._focusByCssSelector(`h1, h2, h3, h4, h5, h6, [role="heading"]`);break;default:this._focusByCssSelector(this.autoFocus);break}}_restoreFocus(t){this.autoFocus!==`dialog`&&(this._elementFocusedBeforeDrawerWasOpened?this._focusMonitor.focusVia(this._elementFocusedBeforeDrawerWasOpened,t):this._elementRef.nativeElement.blur(),this._elementFocusedBeforeDrawerWasOpened=null)}_isFocusWithinDrawer(){let t=this._doc.activeElement;return!!t&&this._elementRef.nativeElement.contains(t)}ngAfterViewInit(){this._isAttached=!0,this._position===`end`&&this._updatePositionInParent(`end`),this._platform.isBrowser&&(this._focusTrap=this._focusTrapFactory.create(this._elementRef.nativeElement),this._updateFocusTrapState())}ngOnDestroy(){this._eventCleanups.forEach(t=>t()),this._focusTrap?.destroy(),this._anchor?.remove(),this._anchor=null,this._animationStarted.complete(),this._animationEnd.complete(),this._modeChanged.complete(),this._destroyed.next(),this._destroyed.complete()}open(t){return this.toggle(!0,t)}close(){return this.toggle(!1)}_closeViaBackdropClick(){return this._setOpen(!1,!0,`mouse`)}toggle(t=!this.opened,e){t&&e&&(this._openedVia=e);let i=this._setOpen(t,!t&&this._isFocusWithinDrawer(),this._openedVia||`program`);return t||(this._openedVia=null),i}_setOpen(t,e,i){return t===this.opened?Promise.resolve(t?`open`:`close`):(this._opened.set(t),this._getContent()?._drawerToggled(this),this._container?._transitionsEnabled?this._isAnimating?(this._setIsAnimating(!1),this._simulateAnimation()):(this._setIsAnimating(!0),setTimeout(()=>this._animationStarted.next())):this._simulateAnimation(),this._elementRef.nativeElement.classList.toggle(`mat-drawer-opened`,t),!t&&e&&this._restoreFocus(i),this._changeDetectorRef.markForCheck(),this._updateFocusTrapState(),new Promise(a=>{this.openedChange.pipe(Et$1(1)).subscribe(r=>a(r?`open`:`close`))}))}_getContent(){return this._container?._content||this._container?._userContent}_setIsAnimating(t){t!==this._isAnimating&&(this._isAnimating=t,this._elementRef.nativeElement.classList.toggle(`mat-drawer-animating`,t))}_simulateAnimation(){setTimeout(()=>{this._animationStarted.next(),this._animationEnd.next()})}_getWidth(){return this._elementRef.nativeElement.offsetWidth||0}_updateFocusTrapState(){this._focusTrap&&(this._focusTrap.enabled=this.opened&&!!this._container?._isShowingBackdrop())}_updatePositionInParent(t){if(!this._platform.isBrowser)return;let e=this._elementRef.nativeElement,i=e.parentNode;t===`end`?(this._anchor||(this._anchor=this._doc.createComment(`mat-drawer-anchor`),i.insertBefore(this._anchor,e)),i.appendChild(e)):this._anchor&&this._anchor.parentNode.insertBefore(e,this._anchor)}_handleTransitionEvent=t=>{let e=this._elementRef.nativeElement;t.target===e&&this._ngZone.run(()=>{t.type===`transitionend`&&this._setIsAnimating(!1),this._animationEnd.next(t)})};static ɵfac=function(e){return new(e||n)};static ɵcmp=FI({type:n,selectors:[[`mat-drawer`]],viewQuery:function(e,i){if(e&1&&Mp(on,5),e&2){let a;SE(a=xE())&&(i._content=a.first)}},hostAttrs:[1,`mat-drawer`],hostVars:12,hostBindings:function(e,i){e&2&&(hp(`align`,null)(`tabIndex`,i.mode!==`side`?`-1`:null),Op(`visibility`,!i._container&&!i.opened?`hidden`:null),Rp(`mat-drawer-end`,i.position===`end`)(`mat-drawer-over`,i.mode===`over`)(`mat-drawer-push`,i.mode===`push`)(`mat-drawer-side`,i.mode===`side`))},inputs:{position:`position`,mode:`mode`,disableClose:`disableClose`,autoFocus:`autoFocus`,opened:`opened`},outputs:{openedChange:`openedChange`,_openedStream:`opened`,openedStart:`openedStart`,_closedStream:`closed`,closedStart:`closedStart`,onPositionChanged:`positionChanged`},exportAs:[`matDrawer`],ngContentSelectors:ye,decls:3,vars:0,consts:[[`content`,``],[`cdkScrollable`,``,1,`mat-drawer-inner-container`]],template:function(e,i){e&1&&(_E(),pi(0,`div`,1,0),ME(2),Mc())},dependencies:[Gi$1],encapsulation:2})}return n})();var Ye=(()=>{class n{_dir=w(gt,{optional:!0});_element=w(vr);_ngZone=w(_e);_changeDetectorRef=w(kP);_animationDisabled=lt();_transitionsEnabled=!1;_allDrawers;_drawers=new ei$1;_content;_userContent;get start(){return this._start}get end(){return this._end}get autosize(){return this._autosize}set autosize(t){this._autosize=as(t)}_autosize=w(dn);get hasBackdrop(){return this._drawerHasBackdrop(this._start)||this._drawerHasBackdrop(this._end)}set hasBackdrop(t){this._backdropOverride=t==null?null:as(t)}_backdropOverride=null;backdropClick=new He;_start=null;_end=null;_left=null;_right=null;_destroyed=new Y;_doCheckSubject=new Y;_contentMargins={left:null,right:null};_contentMarginChanges=new Y;get scrollable(){return this._userContent||this._content}_injector=w(ge);constructor(){let t=w(b),e=w(ct$1);this._dir?.change.pipe(Jh(this._destroyed)).subscribe(()=>{this._validateDrawers(),this.updateContentMargins()}),e.change().pipe(Jh(this._destroyed)).subscribe(()=>this.updateContentMargins()),!this._animationDisabled&&t.isBrowser&&this._ngZone.runOutsideAngular(()=>{setTimeout(()=>{this._element.nativeElement.classList.add(`mat-drawer-transition`),this._transitionsEnabled=!0},200)})}ngAfterContentInit(){this._allDrawers.changes.pipe(Kh(this._allDrawers),Jh(this._destroyed)).subscribe(t=>{this._drawers.reset(t.filter(e=>!e._container||e._container===this)),this._drawers.notifyOnChanges()}),this._drawers.changes.pipe(Kh(null)).subscribe(()=>{this._validateDrawers(),this._drawers.forEach(t=>{this._watchDrawerToggle(t),this._watchDrawerPosition(t),this._watchDrawerMode(t)}),(!this._drawers.length||this._isDrawerOpen(this._start)||this._isDrawerOpen(this._end))&&this.updateContentMargins(),this._changeDetectorRef.markForCheck()}),this._ngZone.runOutsideAngular(()=>{this._doCheckSubject.pipe(Xc(10),Jh(this._destroyed)).subscribe(()=>this.updateContentMargins())})}ngOnDestroy(){this._contentMarginChanges.complete(),this._doCheckSubject.complete(),this._drawers.destroy(),this._destroyed.next(),this._destroyed.complete()}open(){this._drawers.forEach(t=>t.open())}close(){this._drawers.forEach(t=>t.close())}updateContentMargins(){let t=0,e=0;if(this._left&&this._left.opened){if(this._left.mode==`side`)t+=this._left._getWidth();else if(this._left.mode==`push`){let i=this._left._getWidth();t+=i,e-=i}}if(this._right&&this._right.opened){if(this._right.mode==`side`)e+=this._right._getWidth();else if(this._right.mode==`push`){let i=this._right._getWidth();e+=i,t-=i}}t=t||null,e=e||null,(t!==this._contentMargins.left||e!==this._contentMargins.right)&&(this._contentMargins={left:t,right:e},this._ngZone.run(()=>this._contentMarginChanges.next(this._contentMargins)))}ngDoCheck(){this._autosize&&this._isPushed()&&this._ngZone.runOutsideAngular(()=>this._doCheckSubject.next())}_watchDrawerToggle(t){t._animationStarted.pipe(Jh(this._drawers.changes)).subscribe(()=>{this.updateContentMargins(),this._changeDetectorRef.markForCheck()}),t.mode!==`side`&&t.openedChange.pipe(Jh(this._drawers.changes)).subscribe(()=>this._setContainerClass(t.opened))}_watchDrawerPosition(t){t.onPositionChanged.pipe(Jh(this._drawers.changes)).subscribe(()=>{Py({read:()=>this._validateDrawers()},{injector:this._injector})})}_watchDrawerMode(t){t._modeChanged.pipe(Jh(Hh(this._drawers.changes,this._destroyed))).subscribe(()=>{this.updateContentMargins(),this._changeDetectorRef.markForCheck()})}_setContainerClass(t){let e=this._element.nativeElement.classList,i=`mat-drawer-container-has-open`;t?e.add(i):e.remove(i)}_validateDrawers(){this._start=this._end=null,this._drawers.forEach(t=>{t.position==`end`?(this._end,this._end=t):(this._start,this._start=t)}),this._right=this._left=null,this._dir&&this._dir.value===`rtl`?(this._left=this._end,this._right=this._start):(this._left=this._start,this._right=this._end)}_isPushed(){return this._isDrawerOpen(this._start)&&this._start.mode!=`over`||this._isDrawerOpen(this._end)&&this._end.mode!=`over`}_onBackdropClicked(){this.backdropClick.emit(),this._closeModalDrawersViaBackdrop()}_closeModalDrawersViaBackdrop(){[this._start,this._end].filter(t=>t&&!t.disableClose&&this._drawerHasBackdrop(t)).forEach(t=>t._closeViaBackdropClick())}_isShowingBackdrop(){return this._isDrawerOpen(this._start)&&this._drawerHasBackdrop(this._start)||this._isDrawerOpen(this._end)&&this._drawerHasBackdrop(this._end)}_isDrawerOpen(t){return t!=null&&t.opened}_drawerHasBackdrop(t){return this._backdropOverride==null?!!t&&t.mode!==`side`:this._backdropOverride}static ɵfac=function(e){return new(e||n)};static ɵcmp=FI({type:n,selectors:[[`mat-drawer-container`]],contentQueries:function(e,i,a){if(e&1&&_p(a,Vt,5)(a,qe,5),e&2){let r;SE(r=xE())&&(i._content=r.first),SE(r=xE())&&(i._allDrawers=r)}},viewQuery:function(e,i){if(e&1&&Mp(Vt,5),e&2){let a;SE(a=xE())&&(i._userContent=a.first)}},hostAttrs:[1,`mat-drawer-container`],hostVars:2,hostBindings:function(e,i){e&2&&Rp(`mat-drawer-container-explicit-backdrop`,i._backdropOverride)},inputs:{autosize:`autosize`,hasBackdrop:`hasBackdrop`},outputs:{backdropClick:`backdropClick`},exportAs:[`matDrawerContainer`],features:[uD([{provide:Ke,useExisting:n}])],ngContentSelectors:zi,decls:4,vars:2,consts:[[1,`mat-drawer-backdrop`,3,`mat-drawer-shown`],[1,`mat-drawer-backdrop`,3,`click`]],template:function(e,i){e&1&&(_E(Bi),iE(0,rn,1,2,`div`,0),ME(1),ME(2,1),iE(3,sn,2,0,`mat-drawer-content`)),e&2&&(sE(i.hasBackdrop?0:-1),av(3),sE(i._content?-1:3))},dependencies:[Vt],styles:[`.mat-drawer-container {
  position: relative;
  z-index: 1;
  color: var(--%NS%mat-sidenav-content-text-color, var(--%NS%mat-sys-on-background));
  background-color: var(--%NS%mat-sidenav-content-background-color, var(--%NS%mat-sys-background));
  box-sizing: border-box;
  display: block;
  overflow: hidden;
}
.mat-drawer-container[fullscreen] {
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  position: absolute;
}
.mat-drawer-container[fullscreen].mat-drawer-container-has-open {
  overflow: hidden;
}
.mat-drawer-container.mat-drawer-container-explicit-backdrop .mat-drawer-side {
  z-index: 3;
}
.mat-drawer-container.ng-animate-disabled .mat-drawer-backdrop,
.mat-drawer-container.ng-animate-disabled .mat-drawer-content, .ng-animate-disabled .mat-drawer-container .mat-drawer-backdrop,
.ng-animate-disabled .mat-drawer-container .mat-drawer-content {
  transition: none;
}

.mat-drawer-backdrop {
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  position: absolute;
  display: block;
  z-index: 3;
  visibility: hidden;
}
.mat-drawer-backdrop.mat-drawer-shown {
  visibility: visible;
  background-color: var(--%NS%mat-sidenav-scrim-color, color-mix(in srgb, var(--%NS%mat-sys-neutral-variant20) 40%, transparent));
}
.mat-drawer-transition .mat-drawer-backdrop {
  transition-duration: 400ms;
  transition-timing-function: cubic-bezier(0.25, 0.8, 0.25, 1);
  transition-property: background-color, visibility;
}
@media (forced-colors: active) {
  .mat-drawer-backdrop {
    opacity: 0.5;
  }
}

.mat-drawer-content {
  position: relative;
  z-index: 1;
  display: block;
  height: 100%;
  overflow: auto;
}
.mat-drawer-content.mat-drawer-content-hidden {
  opacity: 0;
}
.mat-drawer-transition .mat-drawer-content {
  transition-duration: 400ms;
  transition-timing-function: cubic-bezier(0.25, 0.8, 0.25, 1);
  transition-property: transform, margin-left, margin-right;
}

.mat-drawer {
  position: relative;
  z-index: 4;
  color: var(--%NS%mat-sidenav-container-text-color, var(--%NS%mat-sys-on-surface-variant));
  box-shadow: var(--%NS%mat-sidenav-container-elevation-shadow, none);
  background-color: var(--%NS%mat-sidenav-container-background-color, var(--%NS%mat-sys-surface));
  border-top-right-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-bottom-right-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  width: var(--%NS%mat-sidenav-container-width, 360px);
  display: block;
  position: absolute;
  top: 0;
  bottom: 0;
  z-index: 3;
  outline: 0;
  box-sizing: border-box;
  overflow-y: auto;
  transform: translate3d(-100%, 0, 0);
}
@media (forced-colors: active) {
  .mat-drawer, [dir=rtl] .mat-drawer.mat-drawer-end {
    border-right: solid 1px currentColor;
  }
}
@media (forced-colors: active) {
  [dir=rtl] .mat-drawer, .mat-drawer.mat-drawer-end {
    border-left: solid 1px currentColor;
    border-right: none;
  }
}
.mat-drawer.mat-drawer-side {
  z-index: 2;
}
.mat-drawer.mat-drawer-end {
  right: 0;
  transform: translate3d(100%, 0, 0);
  border-top-left-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-bottom-left-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-top-right-radius: 0;
  border-bottom-right-radius: 0;
}
[dir=rtl] .mat-drawer {
  border-top-left-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-bottom-left-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-top-right-radius: 0;
  border-bottom-right-radius: 0;
  transform: translate3d(100%, 0, 0);
}
[dir=rtl] .mat-drawer.mat-drawer-end {
  border-top-right-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-bottom-right-radius: var(--%NS%mat-sidenav-container-shape, var(--%NS%mat-sys-corner-large));
  border-top-left-radius: 0;
  border-bottom-left-radius: 0;
  left: 0;
  right: auto;
  transform: translate3d(-100%, 0, 0);
}
.mat-drawer-transition .mat-drawer {
  transition: transform 400ms cubic-bezier(0.25, 0.8, 0.25, 1);
}
.mat-drawer:not(.mat-drawer-opened):not(.mat-drawer-animating) {
  visibility: hidden;
  box-shadow: none;
}
.mat-drawer:not(.mat-drawer-opened):not(.mat-drawer-animating) .mat-drawer-inner-container {
  display: none;
}
.mat-drawer.mat-drawer-opened.mat-drawer-opened {
  transform: none;
}

.mat-drawer-side {
  box-shadow: none;
  border-right-color: var(--%NS%mat-sidenav-container-divider-color, transparent);
  border-right-width: 1px;
  border-right-style: solid;
}
.mat-drawer-side.mat-drawer-end {
  border-left-color: var(--%NS%mat-sidenav-container-divider-color, transparent);
  border-left-width: 1px;
  border-left-style: solid;
  border-right: none;
}
[dir=rtl] .mat-drawer-side {
  border-left-color: var(--%NS%mat-sidenav-container-divider-color, transparent);
  border-left-width: 1px;
  border-left-style: solid;
  border-right: none;
}
[dir=rtl] .mat-drawer-side.mat-drawer-end {
  border-right-color: var(--%NS%mat-sidenav-container-divider-color, transparent);
  border-right-width: 1px;
  border-right-style: solid;
  border-left: none;
}

.mat-drawer-inner-container {
  width: 100%;
  height: 100%;
  overflow: auto;
}

.mat-sidenav-fixed {
  position: fixed;
}
`],encapsulation:2})}return n})();var ve=(()=>{class n extends Vt{static ɵfac=(()=>{let t;return function(i){return(t||(t=wm(n)))(i||n)}})();static ɵcmp=FI({type:n,selectors:[[`mat-sidenav-content`]],hostAttrs:[1,`mat-drawer-content`,`mat-sidenav-content`],features:[uD([{provide:Gi$1,useExisting:n},{provide:Vt,useExisting:n}]),sp],ngContentSelectors:ye,decls:1,vars:0,template:function(e,i){e&1&&(_E(),ME(0))},encapsulation:2})}return n})();var Xe=(()=>{class n extends qe{get fixedInViewport(){return this._fixedInViewport}set fixedInViewport(t){this._fixedInViewport=as(t)}_fixedInViewport=!1;get fixedTopGap(){return this._fixedTopGap}set fixedTopGap(t){this._fixedTopGap=Ft(t)}_fixedTopGap=0;get fixedBottomGap(){return this._fixedBottomGap}set fixedBottomGap(t){this._fixedBottomGap=Ft(t)}_fixedBottomGap=0;static ɵfac=(()=>{let t;return function(i){return(t||(t=wm(n)))(i||n)}})();static ɵcmp=FI({type:n,selectors:[[`mat-sidenav`]],hostAttrs:[1,`mat-drawer`,`mat-sidenav`],hostVars:16,hostBindings:function(e,i){e&2&&(hp(`tabIndex`,i.mode!==`side`?`-1`:null)(`align`,null),Op(`top`,i.fixedInViewport?i.fixedTopGap:null,`px`)(`bottom`,i.fixedInViewport?i.fixedBottomGap:null,`px`),Rp(`mat-drawer-end`,i.position===`end`)(`mat-drawer-over`,i.mode===`over`)(`mat-drawer-push`,i.mode===`push`)(`mat-drawer-side`,i.mode===`side`)(`mat-sidenav-fixed`,i.fixedInViewport))},inputs:{fixedInViewport:`fixedInViewport`,fixedTopGap:`fixedTopGap`,fixedBottomGap:`fixedBottomGap`},exportAs:[`matSidenav`],features:[uD([{provide:qe,useExisting:n}]),sp],ngContentSelectors:ye,decls:3,vars:0,consts:[[`content`,``],[`cdkScrollable`,``,1,`mat-drawer-inner-container`]],template:function(e,i){e&1&&(_E(),pi(0,`div`,1,0),ME(2),Mc())},dependencies:[Gi$1],encapsulation:2})}return n})();var ji=(()=>{class n extends Ye{_allDrawers=void 0;_content=void 0;static ɵfac=(()=>{let t;return function(i){return(t||(t=wm(n)))(i||n)}})();static ɵcmp=FI({type:n,selectors:[[`mat-sidenav-container`]],contentQueries:function(e,i,a){if(e&1&&_p(a,ve,5)(a,Xe,5),e&2){let r;SE(r=xE())&&(i._content=r.first),SE(r=xE())&&(i._allDrawers=r)}},hostAttrs:[1,`mat-drawer-container`,`mat-sidenav-container`],hostVars:2,hostBindings:function(e,i){e&2&&Rp(`mat-drawer-container-explicit-backdrop`,i._backdropOverride)},exportAs:[`matSidenavContainer`],features:[uD([{provide:Ke,useExisting:n},{provide:Ye,useExisting:n}]),sp],ngContentSelectors:zi,decls:4,vars:2,consts:[[1,`mat-drawer-backdrop`,3,`mat-drawer-shown`],[1,`mat-drawer-backdrop`,3,`click`]],template:function(e,i){e&1&&(_E(Bi),iE(0,ln,1,2,`div`,0),ME(1),ME(2,1),iE(3,cn,2,0,`mat-sidenav-content`)),e&2&&(sE(i.hasBackdrop?0:-1),av(3),sE(i._content?-1:3))},dependencies:[ve],styles:[mn],encapsulation:2})}return n})();var Vi=(()=>{class n{static ɵfac=function(e){return new(e||n)};static ɵmod=VI({type:n});static ɵinj=Rl({imports:[De$1,st,De$1]})}return n})();var Ze=(()=>{class n{get vertical(){return this._vertical}set vertical(t){this._vertical=as(t)}_vertical=!1;get inset(){return this._inset}set inset(t){this._inset=as(t)}_inset=!1;static ɵfac=function(e){return new(e||n)};static ɵcmp=FI({type:n,selectors:[[`mat-divider`]],hostAttrs:[`role`,`separator`,1,`mat-divider`],hostVars:7,hostBindings:function(e,i){e&2&&(hp(`aria-orientation`,i.vertical?`vertical`:`horizontal`),Rp(`mat-divider-vertical`,i.vertical)(`mat-divider-horizontal`,!i.vertical)(`mat-divider-inset`,i.inset))},inputs:{vertical:`vertical`,inset:`inset`},decls:0,vars:0,template:function(e,i){},styles:[`.mat-divider {
  display: block;
  margin: 0;
  border-top-style: solid;
  border-top-color: var(--%NS%mat-divider-color, var(--%NS%mat-sys-outline-variant));
  border-top-width: var(--%NS%mat-divider-width, 1px);
}
.mat-divider.mat-divider-vertical {
  border-top: 0;
  border-right-style: solid;
  border-right-color: var(--%NS%mat-divider-color, var(--%NS%mat-sys-outline-variant));
  border-right-width: var(--%NS%mat-divider-width, 1px);
}
.mat-divider.mat-divider-inset {
  margin-left: 80px;
}
[dir=rtl] .mat-divider.mat-divider-inset {
  margin-left: auto;
  margin-right: 80px;
}
`],encapsulation:2})}return n})();var xe=(()=>{class n{static ɵfac=function(e){return new(e||n)};static ɵmod=VI({type:n});static ɵinj=Rl({imports:[st]})}return n})();var pn=[`*`];var un=`.mdc-list {
  margin: 0;
  padding: 8px 0;
  list-style-type: none;
}
.mdc-list:focus {
  outline: none;
}

.mdc-list-item {
  display: flex;
  position: relative;
  justify-content: flex-start;
  overflow: hidden;
  padding: 0;
  align-items: stretch;
  cursor: pointer;
  padding-left: 16px;
  padding-right: 16px;
  background-color: var(--%NS%mat-list-list-item-container-color, transparent);
  border-radius: var(--%NS%mat-list-list-item-container-shape, var(--%NS%mat-sys-corner-none));
}
.mdc-list-item.mdc-list-item--selected {
  background-color: var(--%NS%mat-list-list-item-selected-container-color);
}
.mdc-list-item:focus {
  outline: 0;
}
.mdc-list-item.mdc-list-item--disabled {
  cursor: auto;
}
.mdc-list-item.mdc-list-item--with-one-line {
  height: var(--%NS%mat-list-list-item-one-line-container-height, 48px);
}
.mdc-list-item.mdc-list-item--with-one-line .mdc-list-item__start {
  align-self: center;
  margin-top: 0;
}
.mdc-list-item.mdc-list-item--with-one-line .mdc-list-item__end {
  align-self: center;
  margin-top: 0;
}
.mdc-list-item.mdc-list-item--with-two-lines {
  height: var(--%NS%mat-list-list-item-two-line-container-height, 64px);
}
.mdc-list-item.mdc-list-item--with-two-lines .mdc-list-item__start {
  align-self: flex-start;
  margin-top: 16px;
}
.mdc-list-item.mdc-list-item--with-two-lines .mdc-list-item__end {
  align-self: center;
  margin-top: 0;
}
.mdc-list-item.mdc-list-item--with-three-lines {
  height: var(--%NS%mat-list-list-item-three-line-container-height, 88px);
}
.mdc-list-item.mdc-list-item--with-three-lines .mdc-list-item__start {
  align-self: flex-start;
  margin-top: 16px;
}
.mdc-list-item.mdc-list-item--with-three-lines .mdc-list-item__end {
  align-self: flex-start;
  margin-top: 16px;
}
.mdc-list-item.mdc-list-item--%NS%selected::before, .mdc-list-item.mdc-list-item--%NS%selected:focus::before, .mdc-list-item:not(.mdc-list-item--selected):focus::before {
  position: absolute;
  box-sizing: border-box;
  width: 100%;
  height: 100%;
  top: 0;
  left: 0;
  content: "";
  pointer-events: none;
}

a.mdc-list-item {
  color: inherit;
  text-decoration: none;
}

.mdc-list-item__start {
  fill: currentColor;
  flex-shrink: 0;
  pointer-events: none;
}
.mdc-list-item--with-leading-icon .mdc-list-item__start {
  color: var(--%NS%mat-list-list-item-leading-icon-color, var(--%NS%mat-sys-on-surface-variant));
  width: var(--%NS%mat-list-list-item-leading-icon-size, 24px);
  height: var(--%NS%mat-list-list-item-leading-icon-size, 24px);
  margin-left: 16px;
  margin-right: 32px;
}
[dir=rtl] .mdc-list-item--with-leading-icon .mdc-list-item__start {
  margin-left: 32px;
  margin-right: 16px;
}
.mdc-list-item--%NS%with-leading-icon:hover .mdc-list-item__start {
  color: var(--%NS%mat-list-list-item-hover-leading-icon-color);
}
.mdc-list-item--with-leading-avatar .mdc-list-item__start {
  width: var(--%NS%mat-list-list-item-leading-avatar-size, 40px);
  height: var(--%NS%mat-list-list-item-leading-avatar-size, 40px);
  margin-left: 16px;
  margin-right: 16px;
  border-radius: 50%;
}
.mdc-list-item--with-leading-avatar .mdc-list-item__start, [dir=rtl] .mdc-list-item--with-leading-avatar .mdc-list-item__start {
  margin-left: 16px;
  margin-right: 16px;
  border-radius: 50%;
}

.mdc-list-item__end {
  flex-shrink: 0;
  pointer-events: none;
}
.mdc-list-item--with-trailing-meta .mdc-list-item__end {
  font-family: var(--%NS%mat-list-list-item-trailing-supporting-text-font, var(--%NS%mat-sys-label-small-font));
  line-height: var(--%NS%mat-list-list-item-trailing-supporting-text-line-height, var(--%NS%mat-sys-label-small-line-height));
  font-size: var(--%NS%mat-list-list-item-trailing-supporting-text-size, var(--%NS%mat-sys-label-small-size));
  font-weight: var(--%NS%mat-list-list-item-trailing-supporting-text-weight, var(--%NS%mat-sys-label-small-weight));
  letter-spacing: var(--%NS%mat-list-list-item-trailing-supporting-text-tracking, var(--%NS%mat-sys-label-small-tracking));
}
.mdc-list-item--with-trailing-icon .mdc-list-item__end {
  color: var(--%NS%mat-list-list-item-trailing-icon-color, var(--%NS%mat-sys-on-surface-variant));
  width: var(--%NS%mat-list-list-item-trailing-icon-size, 24px);
  height: var(--%NS%mat-list-list-item-trailing-icon-size, 24px);
}
.mdc-list-item--%NS%with-trailing-icon:hover .mdc-list-item__end {
  color: var(--%NS%mat-list-list-item-hover-trailing-icon-color);
}
.mdc-list-item.mdc-list-item--with-trailing-meta .mdc-list-item__end {
  color: var(--%NS%mat-list-list-item-trailing-supporting-text-color, var(--%NS%mat-sys-on-surface-variant));
}
.mdc-list-item--selected.mdc-list-item--with-trailing-icon .mdc-list-item__end {
  color: var(--%NS%mat-list-list-item-selected-trailing-icon-color, var(--%NS%mat-sys-primary));
}

.mdc-list-item__content {
  text-overflow: ellipsis;
  white-space: nowrap;
  overflow: hidden;
  align-self: center;
  flex: 1;
  pointer-events: none;
}
.mdc-list-item--with-two-lines .mdc-list-item__content, .mdc-list-item--with-three-lines .mdc-list-item__content {
  align-self: stretch;
}

.mdc-list-item__primary-text {
  text-overflow: ellipsis;
  white-space: nowrap;
  overflow: hidden;
  color: var(--%NS%mat-list-list-item-label-text-color, var(--%NS%mat-sys-on-surface));
  font-family: var(--%NS%mat-list-list-item-label-text-font, var(--%NS%mat-sys-body-large-font));
  line-height: var(--%NS%mat-list-list-item-label-text-line-height, var(--%NS%mat-sys-body-large-line-height));
  font-size: var(--%NS%mat-list-list-item-label-text-size, var(--%NS%mat-sys-body-large-size));
  font-weight: var(--%NS%mat-list-list-item-label-text-weight, var(--%NS%mat-sys-body-large-weight));
  letter-spacing: var(--%NS%mat-list-list-item-label-text-tracking, var(--%NS%mat-sys-body-large-tracking));
}
.mdc-list-item:hover .mdc-list-item__primary-text {
  color: var(--%NS%mat-list-list-item-hover-label-text-color, var(--%NS%mat-sys-on-surface));
}
.mdc-list-item:focus .mdc-list-item__primary-text {
  color: var(--%NS%mat-list-list-item-focus-label-text-color, var(--%NS%mat-sys-on-surface));
}
.mdc-list-item--with-two-lines .mdc-list-item__primary-text, .mdc-list-item--with-three-lines .mdc-list-item__primary-text {
  display: block;
  margin-top: 0;
  line-height: normal;
  margin-bottom: -20px;
}
.mdc-list-item--with-two-lines .mdc-list-item__primary-text::before, .mdc-list-item--with-three-lines .mdc-list-item__primary-text::before {
  display: inline-block;
  width: 0;
  height: 28px;
  content: "";
  vertical-align: 0;
}
.mdc-list-item--with-two-lines .mdc-list-item__primary-text::after, .mdc-list-item--with-three-lines .mdc-list-item__primary-text::after {
  display: inline-block;
  width: 0;
  height: 20px;
  content: "";
  vertical-align: -20px;
}

.mdc-list-item__secondary-text {
  text-overflow: ellipsis;
  white-space: nowrap;
  overflow: hidden;
  display: block;
  margin-top: 0;
  color: var(--%NS%mat-list-list-item-supporting-text-color, var(--%NS%mat-sys-on-surface-variant));
  font-family: var(--%NS%mat-list-list-item-supporting-text-font, var(--%NS%mat-sys-body-medium-font));
  line-height: var(--%NS%mat-list-list-item-supporting-text-line-height, var(--%NS%mat-sys-body-medium-line-height));
  font-size: var(--%NS%mat-list-list-item-supporting-text-size, var(--%NS%mat-sys-body-medium-size));
  font-weight: var(--%NS%mat-list-list-item-supporting-text-weight, var(--%NS%mat-sys-body-medium-weight));
  letter-spacing: var(--%NS%mat-list-list-item-supporting-text-tracking, var(--%NS%mat-sys-body-medium-tracking));
}
.mdc-list-item__secondary-text::before {
  display: inline-block;
  width: 0;
  height: 20px;
  content: "";
  vertical-align: 0;
}
.mdc-list-item--with-three-lines .mdc-list-item__secondary-text {
  white-space: normal;
  line-height: 20px;
}
.mdc-list-item--with-overline .mdc-list-item__secondary-text {
  white-space: nowrap;
  line-height: auto;
}

.mdc-list-item--with-leading-radio.mdc-list-item,
.mdc-list-item--with-leading-checkbox.mdc-list-item,
.mdc-list-item--with-leading-icon.mdc-list-item,
.mdc-list-item--with-leading-avatar.mdc-list-item {
  padding-left: 0;
  padding-right: 16px;
}
[dir=rtl] .mdc-list-item--with-leading-radio.mdc-list-item,
[dir=rtl] .mdc-list-item--with-leading-checkbox.mdc-list-item,
[dir=rtl] .mdc-list-item--with-leading-icon.mdc-list-item,
[dir=rtl] .mdc-list-item--with-leading-avatar.mdc-list-item {
  padding-left: 16px;
  padding-right: 0;
}
.mdc-list-item--with-leading-radio.mdc-list-item--with-two-lines .mdc-list-item__primary-text,
.mdc-list-item--with-leading-checkbox.mdc-list-item--with-two-lines .mdc-list-item__primary-text,
.mdc-list-item--with-leading-icon.mdc-list-item--with-two-lines .mdc-list-item__primary-text,
.mdc-list-item--with-leading-avatar.mdc-list-item--with-two-lines .mdc-list-item__primary-text {
  display: block;
  margin-top: 0;
  line-height: normal;
  margin-bottom: -20px;
}
.mdc-list-item--with-leading-radio.mdc-list-item--with-two-lines .mdc-list-item__primary-text::before,
.mdc-list-item--with-leading-checkbox.mdc-list-item--with-two-lines .mdc-list-item__primary-text::before,
.mdc-list-item--with-leading-icon.mdc-list-item--with-two-lines .mdc-list-item__primary-text::before,
.mdc-list-item--with-leading-avatar.mdc-list-item--with-two-lines .mdc-list-item__primary-text::before {
  display: inline-block;
  width: 0;
  height: 32px;
  content: "";
  vertical-align: 0;
}
.mdc-list-item--with-leading-radio.mdc-list-item--with-two-lines .mdc-list-item__primary-text::after,
.mdc-list-item--with-leading-checkbox.mdc-list-item--with-two-lines .mdc-list-item__primary-text::after,
.mdc-list-item--with-leading-icon.mdc-list-item--with-two-lines .mdc-list-item__primary-text::after,
.mdc-list-item--with-leading-avatar.mdc-list-item--with-two-lines .mdc-list-item__primary-text::after {
  display: inline-block;
  width: 0;
  height: 20px;
  content: "";
  vertical-align: -20px;
}
.mdc-list-item--with-leading-radio.mdc-list-item--with-two-lines.mdc-list-item--with-trailing-meta .mdc-list-item__end,
.mdc-list-item--with-leading-checkbox.mdc-list-item--with-two-lines.mdc-list-item--with-trailing-meta .mdc-list-item__end,
.mdc-list-item--with-leading-icon.mdc-list-item--with-two-lines.mdc-list-item--with-trailing-meta .mdc-list-item__end,
.mdc-list-item--with-leading-avatar.mdc-list-item--with-two-lines.mdc-list-item--with-trailing-meta .mdc-list-item__end {
  display: block;
  margin-top: 0;
  line-height: normal;
}
.mdc-list-item--with-leading-radio.mdc-list-item--with-two-lines.mdc-list-item--with-trailing-meta .mdc-list-item__end::before,
.mdc-list-item--with-leading-checkbox.mdc-list-item--with-two-lines.mdc-list-item--with-trailing-meta .mdc-list-item__end::before,
.mdc-list-item--with-leading-icon.mdc-list-item--with-two-lines.mdc-list-item--with-trailing-meta .mdc-list-item__end::before,
.mdc-list-item--with-leading-avatar.mdc-list-item--with-two-lines.mdc-list-item--with-trailing-meta .mdc-list-item__end::before {
  display: inline-block;
  width: 0;
  height: 32px;
  content: "";
  vertical-align: 0;
}

.mdc-list-item--with-trailing-icon.mdc-list-item, [dir=rtl] .mdc-list-item--with-trailing-icon.mdc-list-item {
  padding-left: 0;
  padding-right: 0;
}
.mdc-list-item--with-trailing-icon .mdc-list-item__end {
  margin-left: 16px;
  margin-right: 16px;
}

.mdc-list-item--with-trailing-meta.mdc-list-item {
  padding-left: 16px;
  padding-right: 0;
}
[dir=rtl] .mdc-list-item--with-trailing-meta.mdc-list-item {
  padding-left: 0;
  padding-right: 16px;
}
.mdc-list-item--with-trailing-meta .mdc-list-item__end {
  -webkit-user-select: none;
  user-select: none;
  margin-left: 28px;
  margin-right: 16px;
}
[dir=rtl] .mdc-list-item--with-trailing-meta .mdc-list-item__end {
  margin-left: 16px;
  margin-right: 28px;
}
.mdc-list-item--with-trailing-meta.mdc-list-item--with-three-lines .mdc-list-item__end, .mdc-list-item--with-trailing-meta.mdc-list-item--with-two-lines .mdc-list-item__end {
  display: block;
  line-height: normal;
  align-self: flex-start;
  margin-top: 0;
}
.mdc-list-item--with-trailing-meta.mdc-list-item--with-three-lines .mdc-list-item__end::before, .mdc-list-item--with-trailing-meta.mdc-list-item--with-two-lines .mdc-list-item__end::before {
  display: inline-block;
  width: 0;
  height: 28px;
  content: "";
  vertical-align: 0;
}

.mdc-list-item--with-leading-radio .mdc-list-item__start,
.mdc-list-item--with-leading-checkbox .mdc-list-item__start {
  margin-left: 8px;
  margin-right: 24px;
}
[dir=rtl] .mdc-list-item--with-leading-radio .mdc-list-item__start,
[dir=rtl] .mdc-list-item--with-leading-checkbox .mdc-list-item__start {
  margin-left: 24px;
  margin-right: 8px;
}
.mdc-list-item--with-leading-radio.mdc-list-item--with-two-lines .mdc-list-item__start,
.mdc-list-item--with-leading-checkbox.mdc-list-item--with-two-lines .mdc-list-item__start {
  align-self: flex-start;
  margin-top: 8px;
}

.mdc-list-item--with-trailing-radio.mdc-list-item,
.mdc-list-item--with-trailing-checkbox.mdc-list-item {
  padding-left: 16px;
  padding-right: 0;
}
[dir=rtl] .mdc-list-item--with-trailing-radio.mdc-list-item,
[dir=rtl] .mdc-list-item--with-trailing-checkbox.mdc-list-item {
  padding-left: 0;
  padding-right: 16px;
}
.mdc-list-item--with-trailing-radio.mdc-list-item--with-leading-icon, .mdc-list-item--with-trailing-radio.mdc-list-item--with-leading-avatar,
.mdc-list-item--with-trailing-checkbox.mdc-list-item--with-leading-icon,
.mdc-list-item--with-trailing-checkbox.mdc-list-item--with-leading-avatar {
  padding-left: 0;
}
[dir=rtl] .mdc-list-item--with-trailing-radio.mdc-list-item--with-leading-icon, [dir=rtl] .mdc-list-item--with-trailing-radio.mdc-list-item--with-leading-avatar,
[dir=rtl] .mdc-list-item--with-trailing-checkbox.mdc-list-item--with-leading-icon,
[dir=rtl] .mdc-list-item--with-trailing-checkbox.mdc-list-item--with-leading-avatar {
  padding-right: 0;
}
.mdc-list-item--with-trailing-radio .mdc-list-item__end,
.mdc-list-item--with-trailing-checkbox .mdc-list-item__end {
  margin-left: 24px;
  margin-right: 8px;
}
[dir=rtl] .mdc-list-item--with-trailing-radio .mdc-list-item__end,
[dir=rtl] .mdc-list-item--with-trailing-checkbox .mdc-list-item__end {
  margin-left: 8px;
  margin-right: 24px;
}
.mdc-list-item--with-trailing-radio.mdc-list-item--with-three-lines .mdc-list-item__end,
.mdc-list-item--with-trailing-checkbox.mdc-list-item--with-three-lines .mdc-list-item__end {
  align-self: flex-start;
  margin-top: 8px;
}

.mdc-list-group__subheader {
  margin: 0.75rem 16px;
}

.mdc-list-item--disabled .mdc-list-item__start,
.mdc-list-item--disabled .mdc-list-item__content,
.mdc-list-item--disabled .mdc-list-item__end {
  opacity: 1;
}
.mdc-list-item--disabled .mdc-list-item__primary-text,
.mdc-list-item--disabled .mdc-list-item__secondary-text {
  opacity: var(--%NS%mat-list-list-item-disabled-label-text-opacity, 0.3);
}
.mdc-list-item--disabled.mdc-list-item--with-leading-icon .mdc-list-item__start {
  color: var(--%NS%mat-list-list-item-disabled-leading-icon-color, var(--%NS%mat-sys-on-surface));
  opacity: var(--%NS%mat-list-list-item-disabled-leading-icon-opacity, 0.38);
}
.mdc-list-item--disabled.mdc-list-item--with-trailing-icon .mdc-list-item__end {
  color: var(--%NS%mat-list-list-item-disabled-trailing-icon-color, var(--%NS%mat-sys-on-surface));
  opacity: var(--%NS%mat-list-list-item-disabled-trailing-icon-opacity, 0.38);
}

.mat-mdc-list-item.mat-mdc-list-item-both-leading-and-trailing, [dir=rtl] .mat-mdc-list-item.mat-mdc-list-item-both-leading-and-trailing {
  padding-left: 0;
  padding-right: 0;
}

.mdc-list-item.mdc-list-item--disabled .mdc-list-item__primary-text {
  color: var(--%NS%mat-list-list-item-disabled-label-text-color, var(--%NS%mat-sys-on-surface));
}

.mdc-list-item:hover::before {
  background-color: var(--%NS%mat-list-list-item-hover-state-layer-color, var(--%NS%mat-sys-on-surface));
  opacity: var(--%NS%mat-list-list-item-hover-state-layer-opacity, var(--%NS%mat-sys-hover-state-layer-opacity));
}

.mdc-list-item.mdc-list-item--%NS%disabled::before {
  background-color: var(--%NS%mat-list-list-item-disabled-state-layer-color, var(--%NS%mat-sys-on-surface));
  opacity: var(--%NS%mat-list-list-item-disabled-state-layer-opacity, var(--%NS%mat-sys-focus-state-layer-opacity));
}

.mdc-list-item:focus::before {
  background-color: var(--%NS%mat-list-list-item-focus-state-layer-color, var(--%NS%mat-sys-on-surface));
  opacity: var(--%NS%mat-list-list-item-focus-state-layer-opacity, var(--%NS%mat-sys-focus-state-layer-opacity));
}

.mdc-list-item--disabled .mdc-radio,
.mdc-list-item--disabled .mdc-checkbox {
  opacity: var(--%NS%mat-list-list-item-disabled-label-text-opacity, 0.3);
}

.mdc-list-item--with-leading-avatar .mat-mdc-list-item-avatar {
  border-radius: var(--%NS%mat-list-list-item-leading-avatar-shape, var(--%NS%mat-sys-corner-full));
  background-color: var(--%NS%mat-list-list-item-leading-avatar-color, var(--%NS%mat-sys-primary-container));
}

.mat-mdc-list-item-icon {
  font-size: var(--%NS%mat-list-list-item-leading-icon-size, 24px);
}

@media (forced-colors: active) {
  a.mdc-list-item--%NS%activated::after {
    content: "";
    position: absolute;
    top: 50%;
    right: 16px;
    transform: translateY(-50%);
    width: 10px;
    height: 0;
    border-bottom: solid 10px;
    border-radius: 10px;
  }
  a.mdc-list-item--activated [dir=rtl]::after {
    right: auto;
    left: 16px;
  }
}

.mat-mdc-list-base {
  display: block;
}
.mat-mdc-list-base .mdc-list-item__start,
.mat-mdc-list-base .mdc-list-item__end,
.mat-mdc-list-base .mdc-list-item__content {
  pointer-events: auto;
}

.mat-mdc-list-item,
.mat-mdc-list-option {
  width: 100%;
  box-sizing: border-box;
  -webkit-tap-highlight-color: transparent;
}
.mat-mdc-list-item:not(.mat-mdc-list-item-interactive),
.mat-mdc-list-option:not(.mat-mdc-list-item-interactive) {
  cursor: default;
}
.mat-mdc-list-item .mat-divider-inset,
.mat-mdc-list-option .mat-divider-inset {
  position: absolute;
  left: 0;
  right: 0;
  bottom: 0;
}
.mat-mdc-list-item .mat-mdc-list-item-avatar ~ .mat-divider-inset,
.mat-mdc-list-option .mat-mdc-list-item-avatar ~ .mat-divider-inset {
  margin-left: 72px;
}
[dir=rtl] .mat-mdc-list-item .mat-mdc-list-item-avatar ~ .mat-divider-inset,
[dir=rtl] .mat-mdc-list-option .mat-mdc-list-item-avatar ~ .mat-divider-inset {
  margin-right: 72px;
}

.mat-mdc-list-item-interactive::before {
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  position: absolute;
  content: "";
  opacity: 0;
  pointer-events: none;
  border-radius: inherit;
}

.mat-mdc-list-item > .mat-focus-indicator {
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  position: absolute;
  pointer-events: none;
}
.mat-mdc-list-item:focus-visible > .mat-focus-indicator::before {
  content: "";
}

.mat-mdc-list-item.mdc-list-item--with-three-lines .mat-mdc-list-item-line.mdc-list-item__secondary-text {
  white-space: nowrap;
  line-height: normal;
}
.mat-mdc-list-item.mdc-list-item--with-three-lines .mat-mdc-list-item-unscoped-content.mdc-list-item__secondary-text {
  display: -webkit-box;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
}

mat-action-list button {
  background: none;
  color: inherit;
  border: none;
  font: inherit;
  outline: inherit;
  -webkit-tap-highlight-color: transparent;
  text-align: start;
}
mat-action-list button::-moz-focus-inner {
  border: 0;
}

.mdc-list-item--with-leading-icon .mdc-list-item__start {
  margin-inline-start: var(--%NS%mat-list-list-item-leading-icon-start-space, 16px);
  margin-inline-end: var(--%NS%mat-list-list-item-leading-icon-end-space, 16px);
}

.mat-mdc-nav-list .mat-mdc-list-item {
  border-radius: var(--%NS%mat-list-active-indicator-shape, var(--%NS%mat-sys-corner-full));
  --%NS%mat-focus-indicator-border-radius: var(--%NS%mat-list-active-indicator-shape, var(--%NS%mat-sys-corner-full));
}
.mat-mdc-nav-list .mat-mdc-list-item.mdc-list-item--activated {
  background-color: var(--%NS%mat-list-active-indicator-color, var(--%NS%mat-sys-secondary-container));
}
`;var _n=[`unscopedContent`];var gn=[`text`];var fn=[[[``,`matListItemAvatar`,``],[``,`matListItemIcon`,``]],[[``,`matListItemTitle`,``]],[[``,`matListItemLine`,``]],`*`,[[``,`matListItemMeta`,``]],[[`mat-divider`]]];var bn=[`[matListItemAvatar],[matListItemIcon]`,`[matListItemTitle]`,`[matListItemLine]`,`*`,`[matListItemMeta]`,`mat-divider`];var vn=new S(`ListOption`);var Ht=(()=>{class n{_elementRef=w(vr);static ɵfac=function(e){return new(e||n)};static ɵdir=$I({type:n,selectors:[[``,`matListItemTitle`,``]],hostAttrs:[1,`mat-mdc-list-item-title`,`mdc-list-item__primary-text`]})}return n})();var yn=(()=>{class n{_elementRef=w(vr);static ɵfac=function(e){return new(e||n)};static ɵdir=$I({type:n,selectors:[[``,`matListItemLine`,``]],hostAttrs:[1,`mat-mdc-list-item-line`,`mdc-list-item__secondary-text`]})}return n})();var xn=(()=>{class n{static ɵfac=function(e){return new(e||n)};static ɵdir=$I({type:n,selectors:[[``,`matListItemMeta`,``]],hostAttrs:[1,`mat-mdc-list-item-meta`,`mdc-list-item__end`]})}return n})();var Hi=(()=>{class n{_listOption=w(vn,{optional:!0});_isAlignedAtStart(){return!this._listOption||this._listOption?._getTogglePosition()===`after`}static ɵfac=function(e){return new(e||n)};static ɵdir=$I({type:n,hostVars:4,hostBindings:function(e,i){e&2&&Rp(`mdc-list-item__start`,i._isAlignedAtStart())(`mdc-list-item__end`,!i._isAlignedAtStart())}})}return n})();var wn=(()=>{class n extends Hi{static ɵfac=(()=>{let t;return function(i){return(t||(t=wm(n)))(i||n)}})();static ɵdir=$I({type:n,selectors:[[``,`matListItemAvatar`,``]],hostAttrs:[1,`mat-mdc-list-item-avatar`],features:[sp]})}return n})();var Qt=(()=>{class n extends Hi{static ɵfac=(()=>{let t;return function(i){return(t||(t=wm(n)))(i||n)}})();static ɵdir=$I({type:n,selectors:[[``,`matListItemIcon`,``]],hostAttrs:[1,`mat-mdc-list-item-icon`],features:[sp]})}return n})();var kn=new S(`MAT_LIST_CONFIG`);var $e=(()=>{class n{_isNonInteractive=!0;get disableRipple(){return this._disableRipple}set disableRipple(t){this._disableRipple=as(t)}_disableRipple=!1;get disabled(){return this._disabled()}set disabled(t){this._disabled.set(as(t))}_disabled=Po(!1);_defaultOptions=w(kn,{optional:!0});static ɵfac=function(e){return new(e||n)};static ɵdir=$I({type:n,hostVars:1,hostBindings:function(e,i){e&2&&hp(`aria-disabled`,i.disabled)},inputs:{disableRipple:`disableRipple`,disabled:`disabled`}})}return n})();var Sn=(()=>{class n{_elementRef=w(vr);_ngZone=w(_e);_listBase=w($e,{optional:!0});_platform=w(b);_hostElement;_isButtonElement;_noopAnimations=lt();_avatars;_icons;set lines(t){this._explicitLines=Ft(t,null),this._updateItemLines(!1)}_explicitLines=null;get disableRipple(){return this.disabled||this._disableRipple||this._noopAnimations||!!this._listBase?.disableRipple}set disableRipple(t){this._disableRipple=as(t)}_disableRipple=!1;get disabled(){return this._disabled()||!!this._listBase?.disabled}set disabled(t){this._disabled.set(as(t))}_disabled=Po(!1);_subscriptions=new G;_rippleRenderer=null;_hasUnscopedTextContent=!1;rippleConfig;get rippleDisabled(){return this.disableRipple||!!this.rippleConfig.disabled}constructor(){w(ct).load(Ln$1);let t=w(Ce$1,{optional:!0});this.rippleConfig=t||{},this._hostElement=this._elementRef.nativeElement,this._isButtonElement=this._hostElement.nodeName.toLowerCase()===`button`,this._listBase&&!this._listBase._isNonInteractive&&this._initInteractiveListItem(),this._isButtonElement&&!this._hostElement.hasAttribute(`type`)&&this._hostElement.setAttribute(`type`,`button`)}ngAfterViewInit(){this._monitorProjectedLinesAndTitle(),this._updateItemLines(!0)}ngOnDestroy(){this._subscriptions.unsubscribe(),this._rippleRenderer!==null&&this._rippleRenderer._removeTriggerEvents()}_hasIconOrAvatar(){return!!(this._avatars.length||this._icons.length)}_initInteractiveListItem(){this._hostElement.classList.add(`mat-mdc-list-item-interactive`),this._rippleRenderer=new yt(this,this._ngZone,this._hostElement,this._platform,w(ge)),this._rippleRenderer.setupTriggerEvents(this._hostElement)}_monitorProjectedLinesAndTitle(){this._ngZone.runOutsideAngular(()=>{this._subscriptions.add(Hh(this._lines.changes,this._titles.changes).subscribe(()=>this._updateItemLines(!1)))})}_updateItemLines(t){if(!this._lines||!this._titles||!this._unscopedContent)return;t&&this._checkDomForUnscopedTextContent();let e=this._explicitLines??this._inferLinesFromContent(),i=this._unscopedContent.nativeElement;if(this._hostElement.classList.toggle(`mat-mdc-list-item-single-line`,e<=1),this._hostElement.classList.toggle(`mdc-list-item--with-one-line`,e<=1),this._hostElement.classList.toggle(`mdc-list-item--with-two-lines`,e===2),this._hostElement.classList.toggle(`mdc-list-item--with-three-lines`,e===3),this._hasUnscopedTextContent){let a=this._titles.length===0&&e===1;i.classList.toggle(`mdc-list-item__primary-text`,a),i.classList.toggle(`mdc-list-item__secondary-text`,!a)}else i.classList.remove(`mdc-list-item__primary-text`),i.classList.remove(`mdc-list-item__secondary-text`)}_inferLinesFromContent(){let t=this._titles.length+this._lines.length;return this._hasUnscopedTextContent&&(t+=1),t}_checkDomForUnscopedTextContent(){this._hasUnscopedTextContent=Array.from(this._unscopedContent.nativeElement.childNodes).filter(t=>t.nodeType!==t.COMMENT_NODE).some(t=>!!(t.textContent&&t.textContent.trim()))}static ɵfac=function(e){return new(e||n)};static ɵdir=$I({type:n,contentQueries:function(e,i,a){if(e&1&&_p(a,wn,4)(a,Qt,4),e&2){let r;SE(r=xE())&&(i._avatars=r),SE(r=xE())&&(i._icons=r)}},hostVars:4,hostBindings:function(e,i){e&2&&(hp(`aria-disabled`,i.disabled)(`disabled`,i._isButtonElement&&i.disabled||null),Rp(`mdc-list-item--disabled`,i.disabled))},inputs:{lines:`lines`,disableRipple:`disableRipple`,disabled:`disabled`}})}return n})();var we=(()=>{class n extends Sn{_lines;_titles;_meta;_unscopedContent;_itemText;get activated(){return this._activated}set activated(t){this._activated=as(t)}_activated=!1;_getAriaCurrent(){return this._hostElement.nodeName===`A`&&this._activated?`page`:null}_hasBothLeadingAndTrailing(){return this._meta.length!==0&&(this._avatars.length!==0||this._icons.length!==0)}static ɵfac=(()=>{let t;return function(i){return(t||(t=wm(n)))(i||n)}})();static ɵcmp=FI({type:n,selectors:[[`mat-list-item`],[`a`,`mat-list-item`,``],[`button`,`mat-list-item`,``]],contentQueries:function(e,i,a){if(e&1&&_p(a,yn,5)(a,Ht,5)(a,xn,5),e&2){let r;SE(r=xE())&&(i._lines=r),SE(r=xE())&&(i._titles=r),SE(r=xE())&&(i._meta=r)}},viewQuery:function(e,i){if(e&1&&Mp(_n,5)(gn,5),e&2){let a;SE(a=xE())&&(i._unscopedContent=a.first),SE(a=xE())&&(i._itemText=a.first)}},hostAttrs:[1,`mat-mdc-list-item`,`mdc-list-item`],hostVars:13,hostBindings:function(e,i){e&2&&(hp(`aria-current`,i._getAriaCurrent()),Rp(`mdc-list-item--activated`,i.activated)(`mdc-list-item--with-leading-avatar`,i._avatars.length!==0)(`mdc-list-item--with-leading-icon`,i._icons.length!==0)(`mdc-list-item--with-trailing-meta`,i._meta.length!==0)(`mat-mdc-list-item-both-leading-and-trailing`,i._hasBothLeadingAndTrailing())(`_mat-animation-noopable`,i._noopAnimations))},inputs:{activated:`activated`},exportAs:[`matListItem`],features:[sp],ngContentSelectors:bn,decls:10,vars:0,consts:[[`unscopedContent`,``],[1,`mdc-list-item__content`],[1,`mat-mdc-list-item-unscoped-content`,3,`cdkObserveContent`],[1,`mat-focus-indicator`]],template:function(e,i){e&1&&(_E(fn),ME(0),pi(1,`span`,1),ME(2,1),ME(3,2),pi(4,`span`,2,0),wp(`cdkObserveContent`,function(){return i._updateItemLines(!0)}),ME(6,3),Mc()(),ME(7,4),ME(8,5),mp(9,`div`,3))},dependencies:[Yo],encapsulation:2})}return n})();var ke=(()=>{class n extends $e{_isNonInteractive=!1;static ɵfac=(()=>{let t;return function(i){return(t||(t=wm(n)))(i||n)}})();static ɵcmp=FI({type:n,selectors:[[`mat-nav-list`]],hostAttrs:[`role`,`navigation`,1,`mat-mdc-nav-list`,`mat-mdc-list-base`,`mdc-list`],exportAs:[`matNavList`],features:[uD([{provide:$e,useExisting:n}]),sp],ngContentSelectors:pn,decls:1,vars:0,template:function(e,i){e&1&&(_E(),ME(0))},styles:[un],encapsulation:2})}return n})();var Se=(()=>{class n{static ɵfac=function(e){return new(e||n)};static ɵmod=VI({type:n});static ɵinj=Rl({imports:[fn$1,zn$1,h,st,xe]})}return n})();var Tn=[[[`mat-icon`],[``,`matMenuItemIcon`,``]],`*`];var An=[`mat-icon, [matMenuItemIcon]`,`*`];function In(n,s){n&1&&(yu(),pi(0,`svg`,2),mp(1,`polygon`,3),Mc())}var On=[`*`];function En(n,s){if(n&1){let t=mE();Nc(0,`div`,0),Cp(`click`,function(){ou(t);return iu(CE().closed.emit(`click`))})(`animationstart`,function(i){ou(t);return iu(CE()._onAnimationStart(i.animationName))})(`animationend`,function(i){ou(t);return iu(CE()._onAnimationDone(i.animationName))})(`animationcancel`,function(i){ou(t);return iu(CE()._onAnimationDone(i.animationName))}),Nc(1,`div`,1),ME(2),Sc()()}if(n&2){let t=CE();UE(t._classList),Rp(`mat-menu-panel-animations-disabled`,t._animationsDisabled)(`mat-menu-panel-exit-animation`,t._panelAnimationState===`void`)(`mat-menu-panel-animating`,t._isAnimating()),Dp(`id`,t.panelId),hp(`aria-label`,t.ariaLabel||null)(`aria-labelledby`,t.ariaLabelledby||null)(`aria-describedby`,t.ariaDescribedby||null)}}var ei=new S(`MAT_MENU_PANEL`);var vt=(()=>{class n{_elementRef=w(vr);_document=w(tr);_focusMonitor=w(Bt);_parentMenu=w(ei,{optional:!0});_changeDetectorRef=w(kP);role=`menuitem`;disabled=!1;disableRipple=!1;_hovered=new Y;_focused=new Y;_highlighted=!1;_triggersSubmenu=!1;constructor(){w(ct).load(Ln$1),this._parentMenu?.addItem?.(this)}focus(t,e){this._focusMonitor&&t?this._focusMonitor.focusVia(this._getHostElement(),t,e):this._getHostElement().focus(e),this._focused.next(this)}ngAfterViewInit(){this._focusMonitor&&this._focusMonitor.monitor(this._elementRef,!1)}ngOnDestroy(){this._focusMonitor&&this._focusMonitor.stopMonitoring(this._elementRef),this._parentMenu&&this._parentMenu.removeItem&&this._parentMenu.removeItem(this),this._hovered.complete(),this._focused.complete()}_getTabIndex(){return this.disabled?`-1`:`0`}_getHostElement(){return this._elementRef.nativeElement}_checkDisabled(t){this.disabled&&(t.preventDefault(),t.stopPropagation())}_handleMouseEnter(){this._hovered.next(this)}getLabel(){let t=this._elementRef.nativeElement.cloneNode(!0),e=t.querySelectorAll(`mat-icon, .material-icons`);for(let i=0;i<e.length;i++)e[i].remove();return t.textContent?.trim()||``}_setHighlighted(t){this._highlighted=t,this._changeDetectorRef.markForCheck()}_setTriggersSubmenu(t){this._triggersSubmenu=t,this._changeDetectorRef.markForCheck()}_hasFocus(){return this._document&&this._document.activeElement===this._getHostElement()}static ɵfac=function(e){return new(e||n)};static ɵcmp=FI({type:n,selectors:[[``,`mat-menu-item`,``]],hostAttrs:[1,`mat-mdc-menu-item`,`mat-focus-indicator`],hostVars:8,hostBindings:function(e,i){e&1&&wp(`click`,function(r){return i._checkDisabled(r)})(`mouseenter`,function(){return i._handleMouseEnter()}),e&2&&(hp(`role`,i.role)(`tabindex`,i._getTabIndex())(`aria-disabled`,i.disabled)(`disabled`,i.disabled||null),Rp(`mat-mdc-menu-item-highlighted`,i._highlighted)(`mat-mdc-menu-item-submenu-trigger`,i._triggersSubmenu))},inputs:{role:`role`,disabled:[2,`disabled`,`disabled`,PP],disableRipple:[2,`disableRipple`,`disableRipple`,PP]},exportAs:[`matMenuItem`],ngContentSelectors:An,decls:5,vars:3,consts:[[1,`mat-mdc-menu-item-text`],[`matRipple`,``,1,`mat-mdc-menu-ripple`,3,`matRippleDisabled`,`matRippleTrigger`],[`viewBox`,`0 0 5 10`,`focusable`,`false`,`aria-hidden`,`true`,1,`mat-mdc-menu-submenu-icon`],[`points`,`0,0 5,5 0,10`]],template:function(e,i){e&1&&(_E(Tn),ME(0),pi(1,`span`,0),ME(2,1),Mc(),mp(3,`div`,1),iE(4,In,2,0,`:svg:svg`,2)),e&2&&(av(3),gp(`matRippleDisabled`,i.disableRipple||i.disabled)(`matRippleTrigger`,i._getHostElement()),av(),sE(i._triggersSubmenu?4:-1))},dependencies:[ys],encapsulation:2})}return n})();var Rn=new S(`MatMenuContent`);var Ln=new S(`mat-menu-default-options`,{providedIn:`root`,factory:()=>({overlapTrigger:!1,xPosition:`after`,yPosition:`below`,backdropClass:`cdk-overlay-transparent-backdrop`})});var ti=`_mat-menu-enter`;var Me=`_mat-menu-exit`;var mt=(()=>{class n{_elementRef=w(vr);_changeDetectorRef=w(kP);_injector=w(ge);_keyManager;_xPosition;_yPosition;_firstItemFocusRef;_exitFallbackTimeout;_animationsDisabled=lt();_allItems;_directDescendantItems=new ei$1;_classList={};_panelAnimationState=`void`;_animationDone=new Y;_isAnimating=Po(!1);parentMenu;direction;overlayPanelClass;backdropClass;ariaLabel;ariaLabelledby;ariaDescribedby;get xPosition(){return this._xPosition}set xPosition(t){this._xPosition=t,this.setPositionClasses()}get yPosition(){return this._yPosition}set yPosition(t){this._yPosition=t,this.setPositionClasses()}templateRef;items;lazyContent;overlapTrigger=!1;hasBackdrop;get panelClass(){return this._previousPanelClass}set panelClass(t){let e=this._previousPanelClass,i=$({},this._classList);e&&e.length&&e.split(` `).forEach(a=>{i[a]=!1}),this._previousPanelClass=t,t&&t.length&&(t.split(` `).forEach(a=>{i[a]=!0}),this._elementRef.nativeElement.className=``),this._classList=i}_previousPanelClass=``;get classList(){return this.panelClass}set classList(t){this.panelClass=t}closed=new He;close=this.closed;panelId=w(gt$1).getId(`mat-menu-panel-`);constructor(){let t=w(Ln);this.overlayPanelClass=t.overlayPanelClass||``,this._xPosition=t.xPosition,this._yPosition=t.yPosition,this.backdropClass=t.backdropClass,this.overlapTrigger=t.overlapTrigger,this.hasBackdrop=t.hasBackdrop}ngOnInit(){this.setPositionClasses()}ngAfterContentInit(){this._updateDirectDescendants(),this._keyManager=new ye$1(this._directDescendantItems).withWrap().withTypeAhead().withHomeAndEnd(),this._keyManager.tabOut.subscribe(()=>this.closed.emit(`tab`)),this._directDescendantItems.changes.pipe(Kh(this._directDescendantItems),gl(t=>Hh(...t.map(e=>e._focused)))).subscribe(t=>this._keyManager.updateActiveItem(t)),this._directDescendantItems.changes.subscribe(t=>{let e=this._keyManager;if(this._panelAnimationState===`enter`&&e.activeItem?._hasFocus()){let i=t.toArray(),a=Math.max(0,Math.min(i.length-1,e.activeItemIndex||0));i[a]&&!i[a].disabled?e.setActiveItem(a):e.setNextItemActive()}})}ngOnDestroy(){this._keyManager?.destroy(),this._directDescendantItems.destroy(),this.closed.complete(),this._firstItemFocusRef?.destroy(),clearTimeout(this._exitFallbackTimeout)}_hovered(){return this._directDescendantItems.changes.pipe(Kh(this._directDescendantItems),gl(e=>Hh(...e.map(i=>i._hovered))))}addItem(t){}removeItem(t){}_handleKeydown(t){let e=t.keyCode,i=this._keyManager;switch(e){case 27:En$1(t)||(t.preventDefault(),this.closed.emit(`keydown`));break;case 37:this.parentMenu&&this.direction===`ltr`&&this.closed.emit(`keydown`);break;case 39:this.parentMenu&&this.direction===`rtl`&&this.closed.emit(`keydown`);break;default:(e===38||e===40)&&i.setFocusOrigin(`keyboard`),i.onKeydown(t);return}}focusFirstItem(t=`program`){this._firstItemFocusRef?.destroy(),this._firstItemFocusRef=Py(()=>{let e=this._resolvePanel();if(!e||!e.contains(document.activeElement)){let i=this._keyManager;i.setFocusOrigin(t).setFirstItemActive(),!i.activeItem&&e&&e.focus()}},{injector:this._injector})}resetActiveItem(){this._keyManager.setActiveItem(-1)}setElevation(t){}setPositionClasses(t=this.xPosition,e=this.yPosition){this._classList=U($({},this._classList),{"mat-menu-before":t===`before`,"mat-menu-after":t===`after`,"mat-menu-above":e===`above`,"mat-menu-below":e===`below`}),this._changeDetectorRef.markForCheck()}_onAnimationDone(t){let e=t===Me;(e||t===ti)&&(e&&(clearTimeout(this._exitFallbackTimeout),this._exitFallbackTimeout=void 0),this._animationDone.next(e?`void`:`enter`),this._isAnimating.set(!1))}_onAnimationStart(t){(t===ti||t===Me)&&this._isAnimating.set(!0)}_setIsOpen(t){if(this._panelAnimationState=t?`enter`:`void`,t){if(this._keyManager.activeItemIndex===0){let e=this._resolvePanel();e&&(e.scrollTop=0)}}else this._animationsDisabled||(this._exitFallbackTimeout=setTimeout(()=>this._onAnimationDone(Me),200));this._animationsDisabled&&setTimeout(()=>{this._onAnimationDone(t?ti:Me)}),this._changeDetectorRef.markForCheck()}_updateDirectDescendants(){this._allItems.changes.pipe(Kh(this._allItems)).subscribe(t=>{this._directDescendantItems.reset(t.filter(e=>e._parentMenu===this)),this._directDescendantItems.notifyOnChanges()})}_resolvePanel(){let t=null;return this._directDescendantItems.length&&(t=this._directDescendantItems.first._getHostElement().closest(`[role="menu"]`)),t}static ɵfac=function(e){return new(e||n)};static ɵcmp=FI({type:n,selectors:[[`mat-menu`]],contentQueries:function(e,i,a){if(e&1&&_p(a,Rn,5)(a,vt,5)(a,vt,4),e&2){let r;SE(r=xE())&&(i.lazyContent=r.first),SE(r=xE())&&(i._allItems=r),SE(r=xE())&&(i.items=r)}},viewQuery:function(e,i){if(e&1&&Mp(fr,5),e&2){let a;SE(a=xE())&&(i.templateRef=a.first)}},hostVars:3,hostBindings:function(e,i){e&2&&hp(`aria-label`,null)(`aria-labelledby`,null)(`aria-describedby`,null)},inputs:{backdropClass:`backdropClass`,ariaLabel:[0,`aria-label`,`ariaLabel`],ariaLabelledby:[0,`aria-labelledby`,`ariaLabelledby`],ariaDescribedby:[0,`aria-describedby`,`ariaDescribedby`],xPosition:`xPosition`,yPosition:`yPosition`,overlapTrigger:[2,`overlapTrigger`,`overlapTrigger`,PP],hasBackdrop:[2,`hasBackdrop`,`hasBackdrop`,t=>t==null?null:PP(t)],panelClass:[0,`class`,`panelClass`],classList:`classList`},outputs:{closed:`closed`,close:`close`},exportAs:[`matMenu`],features:[uD([{provide:ei,useExisting:n}])],ngContentSelectors:On,decls:1,vars:0,consts:[[`tabindex`,`-1`,`role`,`menu`,1,`mat-mdc-menu-panel`,3,`click`,`animationstart`,`animationend`,`animationcancel`,`id`],[1,`mat-mdc-menu-content`]],template:function(e,i){e&1&&(_E(),lp(0,En,3,12,`ng-template`))},styles:[`mat-menu {
  display: none;
}

.mat-mdc-menu-content {
  margin: 0;
  padding: 8px 0;
  outline: 0;
}
.mat-mdc-menu-content,
.mat-mdc-menu-content .mat-mdc-menu-item .mat-mdc-menu-item-text {
  -moz-osx-font-smoothing: grayscale;
  -webkit-font-smoothing: antialiased;
  flex: 1;
  white-space: normal;
  font-family: var(--%NS%mat-menu-item-label-text-font, var(--%NS%mat-sys-label-large-font));
  line-height: var(--%NS%mat-menu-item-label-text-line-height, var(--%NS%mat-sys-label-large-line-height));
  font-size: var(--%NS%mat-menu-item-label-text-size, var(--%NS%mat-sys-label-large-size));
  letter-spacing: var(--%NS%mat-menu-item-label-text-tracking, var(--%NS%mat-sys-label-large-tracking));
  font-weight: var(--%NS%mat-menu-item-label-text-weight, var(--%NS%mat-sys-label-large-weight));
}

@keyframes _mat-menu-enter {
  from {
    opacity: 0;
    transform: scale(0.8);
  }
  to {
    opacity: 1;
    transform: none;
  }
}
@keyframes _mat-menu-exit {
  from {
    opacity: 1;
  }
  to {
    opacity: 0;
  }
}
.mat-mdc-menu-panel {
  min-width: 112px;
  max-width: 280px;
  overflow: auto;
  box-sizing: border-box;
  outline: 0;
  animation: _mat-menu-enter 120ms cubic-bezier(0, 0, 0.2, 1);
  border-radius: var(--%NS%mat-menu-container-shape, var(--%NS%mat-sys-corner-extra-small));
  background-color: var(--%NS%mat-menu-container-color, var(--%NS%mat-sys-surface-container));
  box-shadow: var(--%NS%mat-menu-container-elevation-shadow, 0px 3px 1px -2px rgba(0, 0, 0, 0.2), 0px 2px 2px 0px rgba(0, 0, 0, 0.14), 0px 1px 5px 0px rgba(0, 0, 0, 0.12));
  will-change: transform, opacity;
}
.mat-mdc-menu-panel.mat-menu-panel-exit-animation {
  animation: _mat-menu-exit 100ms 25ms linear forwards;
}
.mat-mdc-menu-panel.mat-menu-panel-animations-disabled {
  animation: none;
}
.mat-mdc-menu-panel.mat-menu-panel-animating {
  pointer-events: none;
}
.mat-mdc-menu-panel.mat-menu-panel-animating:has(.mat-mdc-menu-content:empty) {
  display: none;
}
@media (forced-colors: active) {
  .mat-mdc-menu-panel {
    outline: solid 1px;
  }
}
.mat-mdc-menu-panel .mat-divider {
  border-top-color: var(--%NS%mat-menu-divider-color, var(--%NS%mat-sys-surface-variant));
  margin-bottom: var(--%NS%mat-menu-divider-bottom-spacing, 8px);
  margin-top: var(--%NS%mat-menu-divider-top-spacing, 8px);
}

.mat-mdc-menu-item {
  display: flex;
  position: relative;
  align-items: center;
  justify-content: flex-start;
  overflow: hidden;
  padding: 0;
  cursor: pointer;
  width: 100%;
  text-align: left;
  box-sizing: border-box;
  color: inherit;
  font-size: inherit;
  background: none;
  text-decoration: none;
  margin: 0;
  min-height: 48px;
  padding-left: var(--%NS%mat-menu-item-leading-spacing, 12px);
  padding-right: var(--%NS%mat-menu-item-trailing-spacing, 12px);
  -webkit-user-select: none;
  user-select: none;
  cursor: pointer;
  outline: none;
  border: none;
  -webkit-tap-highlight-color: transparent;
}
.mat-mdc-menu-item::-moz-focus-inner {
  border: 0;
}
[dir=rtl] .mat-mdc-menu-item {
  padding-left: var(--%NS%mat-menu-item-trailing-spacing, 12px);
  padding-right: var(--%NS%mat-menu-item-leading-spacing, 12px);
}
.mat-mdc-menu-item:has(.material-icons, mat-icon, [matButtonIcon]) {
  padding-left: var(--%NS%mat-menu-item-with-icon-leading-spacing, 12px);
  padding-right: var(--%NS%mat-menu-item-with-icon-trailing-spacing, 12px);
}
[dir=rtl] .mat-mdc-menu-item:has(.material-icons, mat-icon, [matButtonIcon]) {
  padding-left: var(--%NS%mat-menu-item-with-icon-trailing-spacing, 12px);
  padding-right: var(--%NS%mat-menu-item-with-icon-leading-spacing, 12px);
}
.mat-mdc-menu-item, .mat-mdc-menu-item:visited, .mat-mdc-menu-item:link {
  color: var(--%NS%mat-menu-item-label-text-color, var(--%NS%mat-sys-on-surface));
}
.mat-mdc-menu-item .mat-icon-no-color,
.mat-mdc-menu-item .mat-mdc-menu-submenu-icon {
  color: var(--%NS%mat-menu-item-icon-color, var(--%NS%mat-sys-on-surface-variant));
}
.mat-mdc-menu-item[disabled] {
  cursor: default;
  opacity: 0.38;
}
.mat-mdc-menu-item[disabled]::after {
  display: block;
  position: absolute;
  content: "";
  top: 0;
  left: 0;
  bottom: 0;
  right: 0;
}
.mat-mdc-menu-item:focus {
  outline: 0;
}
.mat-mdc-menu-item .mat-icon {
  flex-shrink: 0;
  margin-right: var(--%NS%mat-menu-item-spacing, 12px);
  height: var(--%NS%mat-menu-item-icon-size, 24px);
  width: var(--%NS%mat-menu-item-icon-size, 24px);
}
[dir=rtl] .mat-mdc-menu-item {
  text-align: right;
}
[dir=rtl] .mat-mdc-menu-item .mat-icon {
  margin-right: 0;
  margin-left: var(--%NS%mat-menu-item-spacing, 12px);
}
.mat-mdc-menu-item:not([disabled]):hover {
  background-color: var(--%NS%mat-menu-item-hover-state-layer-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) calc(var(--%NS%mat-sys-hover-state-layer-opacity) * 100%), transparent));
}
.mat-mdc-menu-item:not([disabled]).cdk-program-focused, .mat-mdc-menu-item:not([disabled]).cdk-keyboard-focused, .mat-mdc-menu-item:not([disabled]).mat-mdc-menu-item-highlighted {
  background-color: var(--%NS%mat-menu-item-focus-state-layer-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) calc(var(--%NS%mat-sys-focus-state-layer-opacity) * 100%), transparent));
}
@media (forced-colors: active) {
  .mat-mdc-menu-item {
    margin-top: 1px;
  }
}

.mat-mdc-menu-submenu-icon {
  width: var(--%NS%mat-menu-item-icon-size, 24px);
  height: 10px;
  fill: currentColor;
  padding-left: var(--%NS%mat-menu-item-spacing, 12px);
}
[dir=rtl] .mat-mdc-menu-submenu-icon {
  padding-right: var(--%NS%mat-menu-item-spacing, 12px);
  padding-left: 0;
}
[dir=rtl] .mat-mdc-menu-submenu-icon polygon {
  transform: scaleX(-1);
  transform-origin: center;
}
@media (forced-colors: active) {
  .mat-mdc-menu-submenu-icon {
    fill: CanvasText;
  }
}

.mat-mdc-menu-item .mat-mdc-menu-ripple {
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  position: absolute;
  pointer-events: none;
}
`],encapsulation:2})}return n})();var Pn=new S(`mat-menu-scroll-strategy`,{providedIn:`root`,factory:()=>{let n=w(ge);return()=>qn$1(n)}});var Ot=new WeakMap;var Fn=(()=>{class n{_canHaveBackdrop;_element=w(vr);_viewContainerRef=w(Mi);_menuItemInstance=w(vt,{optional:!0,self:!0});_dir=w(gt,{optional:!0});_focusMonitor=w(Bt);_ngZone=w(_e);_injector=w(ge);_scrollStrategy=w(Pn);_changeDetectorRef=w(kP);_animationsDisabled=lt();_portal;_overlayRef=null;_menuOpen=!1;_closingActionsSubscription=G.EMPTY;_menuCloseSubscription=G.EMPTY;_pendingRemoval;_parentMaterialMenu;_parentInnerPadding;_openedBy=void 0;get _menu(){return this._menuInternal}set _menu(t){t!==this._menuInternal&&(this._menuInternal=t,this._menuCloseSubscription.unsubscribe(),t?(this._parentMaterialMenu,this._menuCloseSubscription=t.close.subscribe(e=>{this._destroyMenu(e),(e===`click`||e===`tab`)&&this._parentMaterialMenu&&this._parentMaterialMenu.closed.emit(e)})):this._destroyMenu(),this._menuItemInstance?._setTriggersSubmenu(this._triggersSubmenu()))}_menuInternal=null;constructor(t){this._canHaveBackdrop=t;let e=w(ei,{optional:!0});this._parentMaterialMenu=e instanceof mt?e:void 0}ngOnDestroy(){this._menu&&this._ownsMenu(this._menu)&&Ot.delete(this._menu),this._pendingRemoval?.unsubscribe(),this._menuCloseSubscription.unsubscribe(),this._closingActionsSubscription.unsubscribe(),this._overlayRef&&(this._overlayRef.dispose(),this._overlayRef=null)}get menuOpen(){return this._menuOpen}get dir(){return this._dir&&this._dir.value===`rtl`?`rtl`:`ltr`}_triggersSubmenu(){return!!(this._menuItemInstance&&this._parentMaterialMenu&&this._menu)}_closeMenu(){this._menu?.close.emit()}_openMenu(t){if(this._triggerIsAriaDisabled())return;let e=this._menu;if(this._menuOpen||!e)return;this._pendingRemoval?.unsubscribe();let i=Ot.get(e);Ot.set(e,this),i&&i!==this&&i._closeMenu();let a=this._createOverlay(e),r=a.getConfig(),d=r.positionStrategy;this._setPosition(e,d),this._canHaveBackdrop?r.hasBackdrop=e.hasBackdrop==null?!this._triggersSubmenu():e.hasBackdrop:r.hasBackdrop=e.hasBackdrop??!1,a.hasAttached()||(a.attach(this._getPortal(e)),e.lazyContent?.attach(this.menuData)),this._closingActionsSubscription=this._menuClosingActions().subscribe(()=>this._closeMenu()),e.parentMenu=this._triggersSubmenu()?this._parentMaterialMenu:void 0,e.direction=this.dir,t&&e.focusFirstItem(this._openedBy||`program`),this._setIsMenuOpen(!0),e instanceof mt&&(e._setIsOpen(!0),e._directDescendantItems.changes.pipe(Jh(e.close)).subscribe(()=>{d.withLockedPosition(!1).reapplyLastPosition(),d.withLockedPosition(!0)}))}focus(t,e){this._focusMonitor&&t?this._focusMonitor.focusVia(this._element,t,e):this._element.nativeElement.focus(e)}_destroyMenu(t){let e=this._overlayRef,i=this._menu;!e||!this.menuOpen||(this._closingActionsSubscription.unsubscribe(),this._pendingRemoval?.unsubscribe(),i instanceof mt&&this._ownsMenu(i)?(this._pendingRemoval=i._animationDone.pipe(Et$1(1)).subscribe(()=>{e.detach(),Ot.has(i)||i.lazyContent?.detach()}),i._setIsOpen(!1)):(e.detach(),i?.lazyContent?.detach()),i&&this._ownsMenu(i)&&Ot.delete(i),this.restoreFocus&&(t===`keydown`||!this._openedBy||!this._triggersSubmenu())&&this.focus(this._openedBy),this._openedBy=void 0,this._setIsMenuOpen(!1))}_setIsMenuOpen(t){t!==this._menuOpen&&(this._menuOpen=t,this._menuOpen?this.menuOpened.emit():this.menuClosed.emit(),this._triggersSubmenu()&&this._menuItemInstance._setHighlighted(t),this._changeDetectorRef.markForCheck())}_createOverlay(t){if(!this._overlayRef){let e=this._getOverlayConfig(t);this._subscribeToPositions(t,e.positionStrategy),this._overlayRef=li(this._injector,e),this._overlayRef.keydownEvents().subscribe(i=>{this._menu instanceof mt&&this._menu._handleKeydown(i)})}return this._overlayRef}_getOverlayConfig(t){return new Jt({positionStrategy:oi(this._injector,this._getOverlayOrigin()).withLockedPosition().withGrowAfterOpen().withTransformOriginOn(`.mat-menu-panel, .mat-mdc-menu-panel`),backdropClass:t.backdropClass||`cdk-overlay-transparent-backdrop`,panelClass:t.overlayPanelClass,scrollStrategy:this._scrollStrategy(),direction:this._dir||`ltr`,disableAnimations:this._animationsDisabled})}_subscribeToPositions(t,e){t.setPositionClasses&&e.positionChanges.subscribe(i=>{this._ngZone.run(()=>{let a=i.connectionPair.overlayX===`start`?`after`:`before`,r=i.connectionPair.overlayY===`top`?`below`:`above`;t.setPositionClasses(a,r)})})}_setPosition(t,e){let[i,a]=t.xPosition===`before`?[`end`,`start`]:[`start`,`end`],[r,d]=t.yPosition===`above`?[`bottom`,`top`]:[`top`,`bottom`],[S,ht]=[r,d],[Ee,Re]=[i,a],Lt=0;if(this._triggersSubmenu()){if(Re=i=t.xPosition===`before`?`start`:`end`,a=Ee=i===`end`?`start`:`end`,this._parentMaterialMenu){if(this._parentInnerPadding==null){let ri=this._parentMaterialMenu.items.first;this._parentInnerPadding=ri?ri._getHostElement().offsetTop:0}Lt=r===`bottom`?this._parentInnerPadding:-this._parentInnerPadding}}else t.overlapTrigger||(S=r===`top`?`bottom`:`top`,ht=d===`top`?`bottom`:`top`);e.withPositions([{originX:i,originY:S,overlayX:Ee,overlayY:r,offsetY:Lt},{originX:a,originY:S,overlayX:Re,overlayY:r,offsetY:Lt},{originX:i,originY:ht,overlayX:Ee,overlayY:d,offsetY:-Lt},{originX:a,originY:ht,overlayX:Re,overlayY:d,offsetY:-Lt}])}_menuClosingActions(){let t=this._getOutsideClickStream(this._overlayRef),e=this._overlayRef.detachments();return Hh(t,this._parentMaterialMenu?this._parentMaterialMenu.closed:kh(),this._parentMaterialMenu?this._parentMaterialMenu._hovered().pipe(Kt(r=>this._menuOpen&&r!==this._menuItemInstance)):kh(),e)}_getPortal(t){return(!this._portal||this._portal.templateRef!==t.templateRef)&&(this._portal=new xt(t.templateRef,this._viewContainerRef)),this._portal}_ownsMenu(t){return Ot.get(t)===this}_triggerIsAriaDisabled(){return PP(this._element.nativeElement.getAttribute(`aria-disabled`))}static ɵfac=function(e){tI()};static ɵdir=$I({type:n})}return n})();var Ce=(()=>{class n extends Fn{_cleanupTouchstart;_hoverSubscription=G.EMPTY;get _deprecatedMatMenuTriggerFor(){return this.menu}set _deprecatedMatMenuTriggerFor(t){this.menu=t}get menu(){return this._menu}set menu(t){this._menu=t}menuData;restoreFocus=!0;menuOpened=new He;onMenuOpen=this.menuOpened;menuClosed=new He;onMenuClose=this.menuClosed;constructor(){super(!0);let t=w(xa);this._cleanupTouchstart=t.listen(this._element.nativeElement,`touchstart`,e=>{ft(e)||(this._openedBy=`touch`)},{passive:!0})}triggersSubmenu(){return super._triggersSubmenu()}toggleMenu(){return this.menuOpen?this.closeMenu():this.openMenu()}openMenu(){this._openMenu(!0)}closeMenu(){this._closeMenu()}updatePosition(){this._overlayRef?.updatePosition()}ngAfterContentInit(){this._handleHover()}ngOnDestroy(){super.ngOnDestroy(),this._cleanupTouchstart(),this._hoverSubscription.unsubscribe()}_getOverlayOrigin(){return this._element}_getOutsideClickStream(t){return t.backdropClick()}_handleMousedown(t){pt(t)||(this._openedBy=t.button===0?`mouse`:void 0,this.triggersSubmenu()&&t.preventDefault())}_handleKeydown(t){let e=t.keyCode;(e===13||e===32)&&(this._openedBy=`keyboard`),this.triggersSubmenu()&&(e===39&&this.dir===`ltr`||e===37&&this.dir===`rtl`)&&(this._openedBy=`keyboard`,this.openMenu())}_handleClick(t){this.triggersSubmenu()?(t.stopPropagation(),this.openMenu()):this.toggleMenu()}_handleHover(){this.triggersSubmenu()&&this._parentMaterialMenu&&(this._hoverSubscription=this._parentMaterialMenu._hovered().subscribe(t=>{t===this._menuItemInstance&&!t.disabled&&this._parentMaterialMenu?._panelAnimationState!==`void`&&(this._openedBy=`mouse`,this._openMenu(!1))}))}static ɵfac=function(e){return new(e||n)};static ɵdir=$I({type:n,selectors:[[``,`mat-menu-trigger-for`,``],[``,`matMenuTriggerFor`,``]],hostAttrs:[1,`mat-mdc-menu-trigger`],hostVars:3,hostBindings:function(e,i){e&1&&wp(`click`,function(r){return i._handleClick(r)})(`mousedown`,function(r){return i._handleMousedown(r)})(`keydown`,function(r){return i._handleKeydown(r)}),e&2&&hp(`aria-haspopup`,i.menu?`menu`:null)(`aria-expanded`,i.menuOpen)(`aria-controls`,i.menuOpen?i.menu?.panelId:null)},inputs:{_deprecatedMatMenuTriggerFor:[0,`mat-menu-trigger-for`,`_deprecatedMatMenuTriggerFor`],menu:[0,`matMenuTriggerFor`,`menu`],menuData:[0,`matMenuTriggerData`,`menuData`],restoreFocus:[0,`matMenuTriggerRestoreFocus`,`restoreFocus`]},outputs:{menuOpened:`menuOpened`,onMenuOpen:`onMenuOpen`,menuClosed:`menuClosed`,onMenuClose:`onMenuClose`},exportAs:[`matMenuTrigger`],features:[sp]})}return n})();var Ne=(()=>{class n{static ɵfac=function(e){return new(e||n)};static ɵmod=VI({type:n});static ɵinj=Rl({imports:[zn$1,Ji$1,st,De$1]})}return n})();function zn(n,s){}var Et=class{viewContainerRef;injector;id;role=`dialog`;panelClass=``;hasBackdrop=!0;backdropClass=``;disableClose=!1;closePredicate;width=``;height=``;minWidth;minHeight;maxWidth;maxHeight;positionStrategy;data=null;direction;ariaDescribedBy=null;ariaLabelledBy=null;ariaLabel=null;ariaModal=!1;autoFocus=`first-tabbable`;restoreFocus=!0;scrollStrategy;closeOnNavigation=!0;closeOnDestroy=!0;closeOnOverlayDetachments=!0;disableAnimations=!1;providers;container;templateContext;bindings};var ni=(()=>{class n extends Zt{_elementRef=w(vr);_focusTrapFactory=w(Ei);_config;_interactivityChecker=w(yn$1);_ngZone=w(_e);_focusMonitor=w(Bt);_renderer=w(xa);_changeDetectorRef=w(kP);_injector=w(ge);_platform=w(b);_document=w(tr);_portalOutlet;_focusTrapped=new Y;_focusTrap=null;_elementFocusedBeforeDialogWasOpened=null;_closeInteractionType=null;_ariaLabelledByQueue=[];_isDestroyed=!1;constructor(){super(),this._config=w(Et,{optional:!0})||new Et,this._config.ariaLabelledBy&&this._ariaLabelledByQueue.push(this._config.ariaLabelledBy)}_addAriaLabelledBy(t){this._ariaLabelledByQueue.push(t),this._changeDetectorRef.markForCheck()}_removeAriaLabelledBy(t){let e=this._ariaLabelledByQueue.indexOf(t);e>-1&&(this._ariaLabelledByQueue.splice(e,1),this._changeDetectorRef.markForCheck())}_contentAttached(){this._initializeFocusTrap(),this._captureInitialFocus()}_captureInitialFocus(){this._trapFocus()}ngOnDestroy(){this._focusTrapped.complete(),this._isDestroyed=!0,this._restoreFocus()}attachComponentPortal(t){this._portalOutlet.hasAttached();let e=this._portalOutlet.attachComponentPortal(t);return this._contentAttached(),e}attachTemplatePortal(t){this._portalOutlet.hasAttached();let e=this._portalOutlet.attachTemplatePortal(t);return this._contentAttached(),e}attachDomPortal=t=>{this._portalOutlet.hasAttached();let e=this._portalOutlet.attachDomPortal(t);return this._contentAttached(),e};_recaptureFocus(){this._containsFocus()||this._trapFocus()}_forceFocus(t,e){this._interactivityChecker.isFocusable(t)||(t.tabIndex=-1,this._ngZone.runOutsideAngular(()=>{let i=()=>{a(),r(),t.removeAttribute(`tabindex`)},a=this._renderer.listen(t,`blur`,i),r=this._renderer.listen(t,`mousedown`,i)})),t.focus(e)}_focusByCssSelector(t,e){let i=this._elementRef.nativeElement.querySelector(t);i&&this._forceFocus(i,e)}_trapFocus(t){this._isDestroyed||Py(()=>{let e=this._elementRef.nativeElement;switch(this._config.autoFocus){case!1:case`dialog`:this._containsFocus()||e.focus(t);break;case!0:case`first-tabbable`:this._focusTrap?.focusInitialElement(t)||this._focusDialogContainer(t);break;case`first-heading`:this._focusByCssSelector(`h1, h2, h3, h4, h5, h6, [role="heading"]`,t);break;default:this._focusByCssSelector(this._config.autoFocus,t);break}this._focusTrapped.next()},{injector:this._injector})}_restoreFocus(){let t=this._config.restoreFocus,e=null;if(typeof t==`string`?e=this._document.querySelector(t):typeof t==`boolean`?e=t?this._elementFocusedBeforeDialogWasOpened:null:t&&(e=t),this._config.restoreFocus&&e&&typeof e.focus==`function`){let i=di(),a=this._elementRef.nativeElement;(!i||i===this._document.body||i===a||a.contains(i))&&(this._focusMonitor?(this._focusMonitor.focusVia(e,this._closeInteractionType),this._closeInteractionType=null):e.focus())}this._focusTrap&&this._focusTrap.destroy()}_focusDialogContainer(t){this._elementRef.nativeElement.focus?.(t)}_containsFocus(){let t=this._elementRef.nativeElement,e=di();return t===e||t.contains(e)}_initializeFocusTrap(){this._platform.isBrowser&&(this._focusTrap=this._focusTrapFactory.create(this._elementRef.nativeElement),this._document&&(this._elementFocusedBeforeDialogWasOpened=di()))}static ɵfac=function(e){return new(e||n)};static ɵcmp=FI({type:n,selectors:[[`cdk-dialog-container`]],viewQuery:function(e,i){if(e&1&&Mp(Ta,7),e&2){let a;SE(a=xE())&&(i._portalOutlet=a.first)}},hostAttrs:[`tabindex`,`-1`,1,`cdk-dialog-container`],hostVars:6,hostBindings:function(e,i){e&2&&hp(`id`,i._config.id||null)(`role`,i._config.role)(`aria-modal`,i._config.ariaModal)(`aria-labelledby`,i._config.ariaLabel?null:i._ariaLabelledByQueue[0])(`aria-label`,i._config.ariaLabel)(`aria-describedby`,i._config.ariaDescribedBy||null)},features:[sp],decls:1,vars:0,consts:[[`cdkPortalOutlet`,``]],template:function(e,i){e&1&&cp(0,zn,0,0,`ng-template`,0)},dependencies:[Ta],styles:[`.cdk-dialog-container {
  display: block;
  width: 100%;
  height: 100%;
  min-height: inherit;
  max-height: inherit;
}
`],encapsulation:2,changeDetection:1})}return n})();var Ut=class{overlayRef;config;componentInstance=null;componentRef=null;containerInstance;disableClose;closed=new Y;backdropClick;keydownEvents;outsidePointerEvents;id;_detachSubscription;constructor(s,t){this.overlayRef=s,this.config=t,this.disableClose=t.disableClose,this.backdropClick=s.backdropClick(),this.keydownEvents=s.keydownEvents(),this.outsidePointerEvents=s.outsidePointerEvents(),this.id=t.id,this.keydownEvents.subscribe(e=>{e.keyCode===27&&!this.disableClose&&!En$1(e)&&(e.preventDefault(),this.close(void 0,{focusOrigin:`keyboard`}))}),this.backdropClick.subscribe(()=>{!this.disableClose&&this._canClose()?this.close(void 0,{focusOrigin:`mouse`}):this.containerInstance._recaptureFocus?.()}),this._detachSubscription=s.detachments().subscribe(()=>{t.closeOnOverlayDetachments!==!1&&this.close()})}close(s,t){if(this._canClose(s)){let e=this.closed;this.containerInstance._closeInteractionType=t?.focusOrigin||`program`,this._detachSubscription.unsubscribe(),this.overlayRef.dispose(),e.next(s),e.complete(),this.componentInstance=this.containerInstance=null}}updatePosition(){return this.overlayRef.updatePosition(),this}updateSize(s=``,t=``){return this.overlayRef.updateSize({width:s,height:t}),this}addPanelClass(s){return this.overlayRef.addPanelClass(s),this}removePanelClass(s){return this.overlayRef.removePanelClass(s),this}_canClose(s){let t=this.config;return!!this.containerInstance&&(!t.closePredicate||t.closePredicate(s,t,this.componentInstance))}};var jn=new S(`DialogScrollStrategy`,{providedIn:`root`,factory:()=>{let n=w(ge);return()=>Gn$1(n)}});var Vn=new S(`DialogData`);var Hn=new S(`DefaultDialogConfig`);function Qn(n){let s=Po(n),t=new He;return{valueSignal:s,get value(){return s()},change:t,ngOnDestroy(){t.complete()}}}var Gi=(()=>{class n{_injector=w(ge);_defaultOptions=w(Hn,{optional:!0});_parentDialog=w(n,{optional:!0,skipSelf:!0});_overlayContainer=w(ii$1);_idGenerator=w(gt$1);_openDialogsAtThisLevel=[];_afterAllClosedAtThisLevel=new Y;_afterOpenedAtThisLevel=new Y;_ariaHiddenElements=new Map;_scrollStrategy=w(jn);get openDialogs(){return this._parentDialog?this._parentDialog.openDialogs:this._openDialogsAtThisLevel}get afterOpened(){return this._parentDialog?this._parentDialog.afterOpened:this._afterOpenedAtThisLevel}afterAllClosed=Vh(()=>this.openDialogs.length?this._getAfterAllClosed():this._getAfterAllClosed().pipe(Kh(void 0)));open(t,e){e=$($({},this._defaultOptions||new Et),e),e.id=e.id||this._idGenerator.getId(`cdk-dialog-`),e.id&&this.getDialogById(e.id);let a=this._getOverlayConfig(e),r=li(this._injector,a),d=new Ut(r,e),S=this._attachContainer(r,d,e);if(d.containerInstance=S,!this.openDialogs.length){let ht=this._overlayContainer.getContainerElement();S._focusTrapped?S._focusTrapped.pipe(Et$1(1)).subscribe(()=>{this._hideNonDialogContentFromAssistiveTechnology(ht)}):this._hideNonDialogContentFromAssistiveTechnology(ht)}return this._attachDialogContent(t,d,S,e),this.openDialogs.push(d),d.closed.subscribe(()=>this._removeOpenDialog(d,!0)),this.afterOpened.next(d),d}closeAll(){ii(this.openDialogs,t=>t.close())}getDialogById(t){return this.openDialogs.find(e=>e.id===t)}ngOnDestroy(){ii(this._openDialogsAtThisLevel,t=>{t.config.closeOnDestroy===!1&&this._removeOpenDialog(t,!1)}),ii(this._openDialogsAtThisLevel,t=>t.close()),this._afterAllClosedAtThisLevel.complete(),this._afterOpenedAtThisLevel.complete(),this._openDialogsAtThisLevel=[]}_getOverlayConfig(t){let e=new Jt({positionStrategy:t.positionStrategy||ri().centerHorizontally().centerVertically(),scrollStrategy:t.scrollStrategy||this._scrollStrategy(),panelClass:t.panelClass,hasBackdrop:t.hasBackdrop,direction:t.direction,minWidth:t.minWidth,minHeight:t.minHeight,maxWidth:t.maxWidth,maxHeight:t.maxHeight,width:t.width,height:t.height,disposeOnNavigation:t.closeOnNavigation,disableAnimations:t.disableAnimations});return t.backdropClass&&(e.backdropClass=t.backdropClass),e}_attachContainer(t,e,i){let a=i.injector||i.viewContainerRef?.injector,r=[{provide:Et,useValue:i},{provide:Ut,useValue:e},{provide:ee,useValue:t}],d;i.container?typeof i.container==`function`?d=i.container:(d=i.container.type,r.push(...i.container.providers(i))):d=ni;let S=new Me$1(d,i.viewContainerRef,ge.create({parent:a||this._injector,providers:r}));return t.attach(S).instance}_attachDialogContent(t,e,i,a){if(t instanceof fr){let r=this._createInjector(a,e,i,void 0),d={$implicit:a.data,dialogRef:e};a.templateContext&&(d=$($({},d),typeof a.templateContext==`function`?a.templateContext():a.templateContext)),i.attachTemplatePortal(new xt(t,null,d,r))}else{let r=this._createInjector(a,e,i,this._injector),d=i.attachComponentPortal(new Me$1(t,a.viewContainerRef,r,null,a.bindings));e.componentRef=d,e.componentInstance=d.instance}}_createInjector(t,e,i,a){let r=t.injector||t.viewContainerRef?.injector,d=[{provide:Vn,useValue:t.data},{provide:Ut,useValue:e}];return t.providers&&(typeof t.providers==`function`?d.push(...t.providers(e,t,i)):d.push(...t.providers)),t.direction&&(!r||!r.get(gt,null,{optional:!0}))&&d.push({provide:gt,useValue:Qn(t.direction)}),ge.create({parent:r||a,providers:d})}_removeOpenDialog(t,e){let i=this.openDialogs.indexOf(t);i>-1&&(this.openDialogs.splice(i,1),this.openDialogs.length||(this._ariaHiddenElements.forEach((a,r)=>{a?r.setAttribute(`aria-hidden`,a):r.removeAttribute(`aria-hidden`)}),this._ariaHiddenElements.clear(),e&&this._getAfterAllClosed().next()))}_hideNonDialogContentFromAssistiveTechnology(t){if(t.parentElement){let e=t.parentElement.children;for(let i=e.length-1;i>-1;i--){let a=e[i];a!==t&&a.nodeName!==`SCRIPT`&&a.nodeName!==`STYLE`&&!a.hasAttribute(`aria-live`)&&!a.hasAttribute(`popover`)&&(this._ariaHiddenElements.set(a,a.getAttribute(`aria-hidden`)),a.setAttribute(`aria-hidden`,`true`))}}}_getAfterAllClosed(){let t=this._parentDialog;return t?t._getAfterAllClosed():this._afterAllClosedAtThisLevel}static ɵfac=function(e){return new(e||n)};static ɵprov=yr({token:n,factory:n.ɵfac})}return n})();function ii(n,s){let t=n.length;for(;t--;)s(n[t])}function Un(n,s){}var qi=`_mat-bottom-sheet-enter`;var Yi=`_mat-bottom-sheet-exit`;var Wn=(()=>{class n extends ni{_breakpointSubscription;_animationsDisabled=lt();_animationState=`void`;_animationStateChanged=new He;_destroyed=!1;constructor(){super();let t=w(ge$1);this._breakpointSubscription=t.observe([ts.Medium,ts.Large,ts.XLarge]).subscribe(()=>{let e=this._elementRef.nativeElement.classList;e.toggle(`mat-bottom-sheet-container-medium`,t.isMatched(ts.Medium)),e.toggle(`mat-bottom-sheet-container-large`,t.isMatched(ts.Large)),e.toggle(`mat-bottom-sheet-container-xlarge`,t.isMatched(ts.XLarge))})}enter(){this._destroyed||(this._animationState=`visible`,this._changeDetectorRef.markForCheck(),this._changeDetectorRef.detectChanges(),this._animationsDisabled&&this._simulateAnimation(qi))}exit(){this._destroyed||(this._elementRef.nativeElement.setAttribute(`mat-exit`,``),this._animationState=`hidden`,this._changeDetectorRef.markForCheck(),this._animationsDisabled&&this._simulateAnimation(Yi))}ngOnDestroy(){super.ngOnDestroy(),this._breakpointSubscription.unsubscribe(),this._destroyed=!0}_simulateAnimation(t){this._ngZone.run(()=>{this._handleAnimationEvent(!0,t,this._elementRef.nativeElement),setTimeout(()=>this._handleAnimationEvent(!1,t,this._elementRef.nativeElement))})}_trapFocus(){super._trapFocus({preventScroll:!0})}_handleAnimationEvent(t,e,i){if(i===this._elementRef.nativeElement){let a=e===qi;(a||e===Yi)&&this._animationStateChanged.emit({toState:a?`visible`:`hidden`,phase:t?`start`:`done`})}}static ɵfac=function(e){return new(e||n)};static ɵcmp=FI({type:n,selectors:[[`mat-bottom-sheet-container`]],hostAttrs:[`tabindex`,`-1`,1,`mat-bottom-sheet-container`],hostVars:9,hostBindings:function(e,i){e&1&&wp(`animationstart`,function(r){return i._handleAnimationEvent(!0,r.animationName,r.target)})(`animationend`,function(r){return i._handleAnimationEvent(!1,r.animationName,r.target)})(`animationcancel`,function(r){return i._handleAnimationEvent(!1,r.animationName,r.target)}),e&2&&(hp(`role`,i._config.role)(`aria-modal`,i._config.ariaModal)(`aria-label`,i._config.ariaLabel),Rp(`mat-bottom-sheet-container-animations-enabled`,!i._animationsDisabled)(`mat-bottom-sheet-container-enter`,i._animationState===`visible`)(`mat-bottom-sheet-container-exit`,i._animationState===`hidden`))},features:[sp],decls:1,vars:0,consts:[[`cdkPortalOutlet`,``]],template:function(e,i){e&1&&cp(0,Un,0,0,`ng-template`,0)},dependencies:[Ta],styles:[`@keyframes _mat-bottom-sheet-enter {
  from {
    transform: translateY(100%);
  }
  to {
    transform: none;
  }
}
@keyframes _mat-bottom-sheet-exit {
  from {
    transform: none;
  }
  to {
    transform: translateY(100%);
  }
}
.mat-bottom-sheet-container {
  box-shadow: 0px 8px 10px -5px rgba(0, 0, 0, 0.2), 0px 16px 24px 2px rgba(0, 0, 0, 0.14), 0px 6px 30px 5px rgba(0, 0, 0, 0.12);
  padding: 8px 16px;
  min-width: 100vw;
  box-sizing: border-box;
  display: block;
  outline: 0;
  max-height: 80vh;
  overflow: auto;
  position: relative;
  background: var(--%NS%mat-bottom-sheet-container-background-color, var(--%NS%mat-sys-surface-container-low));
  color: var(--%NS%mat-bottom-sheet-container-text-color, var(--%NS%mat-sys-on-surface));
  font-family: var(--%NS%mat-bottom-sheet-container-text-font, var(--%NS%mat-sys-body-large-font));
  font-size: var(--%NS%mat-bottom-sheet-container-text-size, var(--%NS%mat-sys-body-large-size));
  line-height: var(--%NS%mat-bottom-sheet-container-text-line-height, var(--%NS%mat-sys-body-large-line-height));
  font-weight: var(--%NS%mat-bottom-sheet-container-text-weight, var(--%NS%mat-sys-body-large-weight));
  letter-spacing: var(--%NS%mat-bottom-sheet-container-text-tracking, var(--%NS%mat-sys-body-large-tracking));
}
@media (forced-colors: active) {
  .mat-bottom-sheet-container {
    outline: 1px solid;
  }
}

.mat-bottom-sheet-container-animations-enabled {
  transform: translateY(100%);
}
.mat-bottom-sheet-container-animations-enabled.mat-bottom-sheet-container-enter {
  animation: _mat-bottom-sheet-enter 195ms cubic-bezier(0, 0, 0.2, 1) forwards;
}
.mat-bottom-sheet-container-animations-enabled.mat-bottom-sheet-container-exit {
  animation: _mat-bottom-sheet-exit 375ms cubic-bezier(0.4, 0, 1, 1) backwards;
}

.mat-bottom-sheet-container-xlarge, .mat-bottom-sheet-container-large, .mat-bottom-sheet-container-medium {
  border-top-left-radius: var(--%NS%mat-bottom-sheet-container-shape, 28px);
  border-top-right-radius: var(--%NS%mat-bottom-sheet-container-shape, 28px);
}

.mat-bottom-sheet-container-medium {
  min-width: 384px;
  max-width: calc(100vw - 128px);
}

.mat-bottom-sheet-container-large {
  min-width: 512px;
  max-width: calc(100vw - 256px);
}

.mat-bottom-sheet-container-xlarge {
  min-width: 576px;
  max-width: calc(100vw - 384px);
}
`],encapsulation:2,changeDetection:1})}return n})();var Gn=new S(`MatBottomSheetData`);var ai=class{viewContainerRef;injector;panelClass;direction;data=null;hasBackdrop=!0;backdropClass;disableClose=!1;ariaLabel=null;ariaModal=!1;closeOnNavigation=!0;autoFocus=`first-tabbable`;restoreFocus=!0;scrollStrategy;height=``;minHeight;maxHeight;bindings};var Rt=class{_ref;get instance(){return this._ref.componentInstance}get componentRef(){return this._ref.componentRef}containerInstance;disableClose;_afterOpened=new Y;_result;_closeFallbackTimeout;constructor(s,t,e){this._ref=s,this.containerInstance=e,this.disableClose=t.disableClose,e._animationStateChanged.pipe(Kt(i=>i.phase===`done`&&i.toState===`visible`),Et$1(1)).subscribe(()=>{this._afterOpened.next(),this._afterOpened.complete()}),e._animationStateChanged.pipe(Kt(i=>i.phase===`done`&&i.toState===`hidden`),Et$1(1)).subscribe(()=>{clearTimeout(this._closeFallbackTimeout),this._ref.close(this._result)}),s.overlayRef.detachments().subscribe(()=>{this._ref.close(this._result)}),Hh(this.backdropClick(),this.keydownEvents().pipe(Kt(i=>i.keyCode===27))).subscribe(i=>{!this.disableClose&&(i.type!==`keydown`||!En$1(i))&&(i.preventDefault(),this.dismiss())})}dismiss(s){this.containerInstance&&(this.containerInstance._animationStateChanged.pipe(Kt(t=>t.phase===`start`),Et$1(1)).subscribe(()=>{this._closeFallbackTimeout=setTimeout(()=>this._ref.close(this._result),500),this._ref.overlayRef.detachBackdrop()}),this._result=s,this.containerInstance.exit(),this.containerInstance=null)}afterDismissed(){return this._ref.closed}afterOpened(){return this._afterOpened}backdropClick(){return this._ref.backdropClick}keydownEvents(){return this._ref.keydownEvents}};var qn=new S(`mat-bottom-sheet-default-options`);var Ki=(()=>{class n{_injector=w(ge);_parentBottomSheet=w(n,{optional:!0,skipSelf:!0});_animationsDisabled=lt();_defaultOptions=w(qn,{optional:!0});_bottomSheetRefAtThisLevel=null;_dialog=w(Gi);get _openedBottomSheetRef(){let t=this._parentBottomSheet;return t?t._openedBottomSheetRef:this._bottomSheetRefAtThisLevel}set _openedBottomSheetRef(t){this._parentBottomSheet?this._parentBottomSheet._openedBottomSheetRef=t:this._bottomSheetRefAtThisLevel=t}open(t,e){let i=$($({},this._defaultOptions||new ai),e),a;return this._dialog.open(t,U($({},i),{disableClose:!0,closeOnOverlayDetachments:!1,maxWidth:`100%`,container:Wn,scrollStrategy:i.scrollStrategy||Gn$1(this._injector),positionStrategy:ri(this._injector).centerHorizontally().bottom(`0`),disableAnimations:this._animationsDisabled,templateContext:()=>({bottomSheetRef:a}),providers:(r,d,S)=>(a=new Rt(r,i,S),[{provide:Rt,useValue:a},{provide:Gn,useValue:i.data}])})),a.afterDismissed().subscribe(()=>{this._openedBottomSheetRef===a&&(this._openedBottomSheetRef=null)}),this._openedBottomSheetRef?(this._openedBottomSheetRef.afterDismissed().subscribe(()=>a.containerInstance?.enter()),this._openedBottomSheetRef.dismiss()):a.containerInstance.enter(),this._openedBottomSheetRef=a,a}dismiss(t){this._openedBottomSheetRef&&this._openedBottomSheetRef.dismiss(t)}ngOnDestroy(){this._bottomSheetRefAtThisLevel&&this._bottomSheetRefAtThisLevel.dismiss()}static ɵfac=function(e){return new(e||n)};static ɵprov=yr({token:n,factory:n.ɵfac})}return n})();function De(n,s){let e=!s?.manualCleanup?s?.injector?.get(me)??w(me):null,i=Yn(s?.equal),a;s?.requireSync?a=Po({kind:0},{equal:i}):a=Po({kind:1,value:s?.initialValue},{equal:i});let r,d=n.subscribe({next:S=>a.set({kind:1,value:S}),error:S=>{a.set({kind:2,error:S}),r?.()},complete:()=>{r?.()}});if(s?.requireSync&&a().kind===0)throw new M(601,!1);return r=e?.onDestroy(d.unsubscribe.bind(d)),TD(()=>{let S=a();switch(S.kind){case 1:return S.value;case 2:throw S.error;case 0:throw new M(601,!1)}},{equal:s?.equal})}function Yn(n=Object.is){return(s,t)=>s.kind===1&&t.kind===1&&n(s.value,t.value)}var dt=class n{auth=w(m);moduleAccess=w(l);sectionKeys=Po([]);loaded=Po(!1);isReady=TD(()=>this.loaded()||this.isAdmin());isAdmin(){return this.auth.currentUser()?.roles.includes(r.admin)??!1}load(){if(!this.auth.isAuthenticated()){this.reset();return}if(this.isAdmin()){this.loaded.set(!0);return}this.moduleAccess.getMySections().subscribe({next:s=>{this.sectionKeys.set(s),this.loaded.set(!0)},error:()=>this.loaded.set(!0)})}reset(){this.sectionKeys.set([]),this.loaded.set(!1)}can(s){return this.isAdmin()?!0:this.sectionKeys().includes(s)}static ɵfac=function(t){return new(t||n)};static ɵprov=ce({token:n,factory:n.ɵfac,providedIn:`root`})};var Te=class n{templateRef=w(fr);viewContainerRef=w(Mi);store=w(dt);sectionKey=``;rendered=!1;constructor(){_u(()=>{this.store.isReady(),this.updateView()})}set key(s){this.sectionKey=s,this.updateView()}updateView(){let s=this.sectionKey!==``&&this.store.can(this.sectionKey);s&&!this.rendered?(this.viewContainerRef.createEmbeddedView(this.templateRef),this.rendered=!0):!s&&this.rendered&&(this.viewContainerRef.clear(),this.rendered=!1)}static ɵfac=function(t){return new(t||n)};static ɵdir=$I({type:n,selectors:[[``,`canRender`,``]],inputs:{key:[0,`canRender`,`key`]}})};var Kn=(n,s)=>s.category;var Xn=(n,s)=>s.link;function Zn(n,s){if(n&1){let t=mE();pi(0,`a`,7),wp(`click`,function(){let i=ou(t).$implicit;return iu(CE(2).navigate(i))}),pi(1,`mat-icon`,8),eD(2),Mc(),pi(3,`span`,9),eD(4),Mc()()}if(n&2){let t=s.$implicit;av(2),Hp(t.icon),av(2),Hp(t.name)}}function $n(n,s){if(n&1&&(pi(0,`p`,4),eD(1),Mc(),pi(2,`mat-nav-list`,5),lE(3,Zn,5,2,`a`,6,Xn),Mc()),n&2){let t=s.$implicit;av(),Hp(t.category),av(2),uE(t.options)}}function Jn(n,s){if(n&1){let t=mE();pi(0,`p`,4),eD(1,`ADMINISTRATION`),Mc(),pi(2,`mat-nav-list`,5)(3,`a`,7),wp(`click`,function(){ou(t);return iu(CE().navigateToWorkflows())}),pi(4,`mat-icon`,8),eD(5,`fact_check`),Mc(),pi(6,`span`,9),eD(7,`Approval Workflows`),Mc()()()}}var ta={"/users":`group`,"/roles":`admin_panel_settings`,"/profile":`person`};var Ae=class n{router=w(ce$1);sheetRef=w(Rt);moduleAccess=w(l);sectionAccess=w(dt);auth=w(m);tree=Po([]);groups=TD(()=>(this.sectionAccess.isReady(),this.tree().filter(s=>s.isActive).map(s=>({category:s.name.toUpperCase(),options:s.pages.filter(t=>t.isActive&&t.sections.some(e=>e.isActive&&this.sectionAccess.can(e.key))).sort((t,e)=>t.sortOrder-e.sortOrder).map(t=>({name:t.name,link:t.url,icon:ta[t.url]??`chevron_right`}))})).filter(s=>s.options.length>0)));constructor(){this.moduleAccess.getTree().subscribe(s=>this.tree.set(s))}navigate(s){this.router.navigate([s.link]),this.sheetRef.dismiss()}navigateToWorkflows(){this.router.navigate([`/approval-workflows`]),this.sheetRef.dismiss()}dismiss(){this.sheetRef.dismiss()}static ɵfac=function(t){return new(t||n)};static ɵcmp=FI({type:n,selectors:[[`app-settings-sheet`]],decls:10,vars:1,consts:[[1,`sheet-container`],[1,`sheet-header`],[1,`mb-0`],[`type`,`button`,`mat-icon-button`,``,`aria-label`,`Close`,3,`click`],[1,`group-label`],[1,`mb-2`],[`mat-list-item`,``],[`mat-list-item`,``,3,`click`],[`matListItemIcon`,``],[`matListItemTitle`,``]],template:function(t,e){t&1&&(pi(0,`section`,0)(1,`div`,1)(2,`h4`,2),eD(3,`Settings`),Mc(),pi(4,`button`,3),wp(`click`,function(){return e.dismiss()}),pi(5,`mat-icon`),eD(6,`close`),Mc()()(),lE(7,$n,5,1,null,null,Kn),iE(9,Jn,8,0),Mc()),t&2&&(av(7),uE(e.groups()),av(2),sE(e.auth.isAdmin()?9:-1))},dependencies:[oe,ne,Ks,Ui,Se,ke,we,Qt,Ht],styles:[`.sheet-container[_ngcontent-%COMP%]{padding:.5rem 1rem 1rem;min-width:320px;max-width:480px}.sheet-header[_ngcontent-%COMP%]{display:flex;align-items:center;justify-content:space-between;padding:.5rem 0}.group-label[_ngcontent-%COMP%]{font-size:.75rem;font-weight:600;letter-spacing:.04em;color:#6b7280;margin:.5rem 0 .25rem}`]})};var na=()=>({exact:!0});function aa(n,s){if(n&1&&(pi(0,`div`,14)(1,`div`,34),eD(2),Mc(),pi(3,`div`,35),eD(4),Mc()()),n&2){let t=s;av(2),Bp(``,t.firstName,` `,t.lastName),av(2),Hp(t.email)}}function oa(n,s){if(n&1){let t=mE();Ip(0),pi(1,`a`,36),wp(`click`,function(){ou(t);let i=CE(),a=OE(40);return iu(i.isScreenSmall()&&a.close())}),pi(2,`mat-icon`,21),eD(3,`group`),Mc(),pi(4,`span`,22),eD(5,`Users`),Mc()(),Ep()}}function ra(n,s){if(n&1){let t=mE();pi(0,`a`,37),wp(`click`,function(){ou(t);let i=CE(),a=OE(40);return iu(i.isScreenSmall()&&a.close())}),pi(1,`mat-icon`,21),eD(2,`admin_panel_settings`),Mc(),pi(3,`span`,22),eD(4,`Roles`),Mc()()}}var Ie=class n{auth=w(m);sectionAccess=w(dt);cartService=w(p);breakpointObserver=w(ge$1);bottomSheet=w(Ki);cartCount=this.cartService.totalItemsCount;constructor(){this.sectionAccess.load()}isScreenSmall=De(this.breakpointObserver.observe(ts.Handset).pipe(Fe(s=>s.matches)),{initialValue:!1});openSettings(){this.bottomSheet.open(Ae)}logout(){this.auth.logout()}static ɵfac=function(t){return new(t||n)};static ɵcmp=FI({type:n,selectors:[[`app-admin-shell`]],decls:87,vars:10,consts:[[`brandImg`,``],[`brandFallback`,``],[`userMenu`,`matMenu`],[`sidenav`,``],[`color`,`primary`,1,`app-toolbar`,`mat-elevation-z4`],[`mat-icon-button`,``,`aria-label`,`Toggle navigation`,3,`click`],[1,`brand`],[`src`,`logo.png`,`alt`,`SRIVIDIKA`,1,`brand-logo`,3,`error`],[1,`brand-text`,`d-none`],[1,`flex-spacer`],[`mat-icon-button`,``,`matTooltip`,`Settings`,`aria-label`,`Settings`,3,`click`],[`mat-icon-button`,``,`routerLink`,`/cart`,`matTooltip`,`Shopping Cart`,`aria-label`,`Shopping Cart`],[`matBadgeColor`,`warn`,3,`matBadge`,`matBadgeHidden`],[`mat-icon-button`,``,`aria-label`,`Account`,3,`matMenuTriggerFor`],[1,`px-3`,`py-2`],[`mat-menu-item`,``,`routerLink`,`/profile`],[`mat-menu-item`,``,`routerLink`,`/orders`],[`mat-menu-item`,``,3,`click`],[1,`app-sidenav-container`],[1,`app-sidenav`,3,`opened`,`mode`],[`mat-list-item`,``,`routerLink`,`/catalog`,`routerLinkActive`,`link-active`,3,`click`],[`matListItemIcon`,``],[`matListItemTitle`,``],[`mat-list-item`,``,`routerLink`,`/cart`,`routerLinkActive`,`link-active`,3,`click`],[`mat-list-item`,``,`routerLink`,`/dashboard`,`routerLinkActive`,`link-active`,3,`click`,`routerLinkActiveOptions`],[`mat-list-item`,``,`routerLink`,`/orders`,`routerLinkActive`,`link-active`,3,`click`],[`mat-list-item`,``,`routerLink`,`/profile`,`routerLinkActive`,`link-active`,3,`click`],[4,`canRender`],[`mat-list-item`,``,`routerLink`,`/roles`,`routerLinkActive`,`link-active`],[`mat-list-item`,``,`routerLink`,`/categories`,`routerLinkActive`,`link-active`,3,`click`],[`mat-list-item`,``,`routerLink`,`/items`,`routerLinkActive`,`link-active`,3,`click`],[`mat-list-item`,``,`routerLink`,`/payment-settings`,`routerLinkActive`,`link-active`,3,`click`],[1,`app-sidenav-content`],[1,`content`],[1,`fw-semibold`],[1,`text-muted`,`small`],[`mat-list-item`,``,`routerLink`,`/users`,`routerLinkActive`,`link-active`,3,`click`],[`mat-list-item`,``,`routerLink`,`/roles`,`routerLinkActive`,`link-active`,3,`click`]],template:function(t,e){if(t&1){let i=mE();pi(0,`mat-toolbar`,4)(1,`button`,5),wp(`click`,function(){ou(i);return iu(OE(40).toggle())}),pi(2,`mat-icon`),eD(3,`menu`),Mc()(),pi(4,`span`,6)(5,`img`,7,0),wp(`error`,function(){ou(i);let r=OE(6),d=OE(8);return r.classList.add(`d-none`),iu(d.classList.remove(`d-none`))}),Mc(),pi(7,`span`,8,1),eD(9,`SRIVIDIKA`),Mc()(),mp(10,`span`,9),pi(11,`button`,10),wp(`click`,function(){return e.openSettings()}),pi(12,`mat-icon`),eD(13,`settings`),Mc()(),pi(14,`a`,11)(15,`mat-icon`,12),eD(16,` shopping_cart `),Mc()(),pi(17,`button`,13)(18,`mat-icon`),eD(19,`account_circle`),Mc()(),pi(20,`mat-menu`,null,2),iE(22,aa,5,3,`div`,14),pi(23,`button`,15)(24,`mat-icon`),eD(25,`person`),Mc(),pi(26,`span`),eD(27,`My Profile`),Mc()(),pi(28,`button`,16)(29,`mat-icon`),eD(30,`receipt_long`),Mc(),pi(31,`span`),eD(32,`My Orders`),Mc()(),pi(33,`button`,17),wp(`click`,function(){return e.logout()}),pi(34,`mat-icon`),eD(35,`logout`),Mc(),pi(36,`span`),eD(37,`Logout`),Mc()()()(),pi(38,`mat-sidenav-container`,18)(39,`mat-sidenav`,19,3)(41,`mat-nav-list`)(42,`a`,20),wp(`click`,function(){ou(i);let r=OE(40);return iu(e.isScreenSmall()&&r.close())}),pi(43,`mat-icon`,21),eD(44,`storefront`),Mc(),pi(45,`span`,22),eD(46,`Product Catalog`),Mc()(),pi(47,`a`,23),wp(`click`,function(){ou(i);let r=OE(40);return iu(e.isScreenSmall()&&r.close())}),pi(48,`mat-icon`,21),eD(49,`shopping_cart`),Mc(),pi(50,`span`,22),eD(51,`Shopping Cart`),Mc()(),pi(52,`a`,24),wp(`click`,function(){ou(i);let r=OE(40);return iu(e.isScreenSmall()&&r.close())}),pi(53,`mat-icon`,21),eD(54,`speed`),Mc(),pi(55,`span`,22),eD(56,`Dashboard`),Mc()(),pi(57,`a`,25),wp(`click`,function(){ou(i);let r=OE(40);return iu(e.isScreenSmall()&&r.close())}),pi(58,`mat-icon`,21),eD(59,`receipt_long`),Mc(),pi(60,`span`,22),eD(61,`Orders`),Mc()(),pi(62,`a`,26),wp(`click`,function(){ou(i);let r=OE(40);return iu(e.isScreenSmall()&&r.close())}),pi(63,`mat-icon`,21),eD(64,`person`),Mc(),pi(65,`span`,22),eD(66,`My Profile`),Mc()(),cp(67,oa,6,0,`ng-container`,27),iE(68,ra,5,0,`a`,28),pi(69,`a`,29),wp(`click`,function(){ou(i);let r=OE(40);return iu(e.isScreenSmall()&&r.close())}),pi(70,`mat-icon`,21),eD(71,`category`),Mc(),pi(72,`span`,22),eD(73,`Categories`),Mc()(),pi(74,`a`,30),wp(`click`,function(){ou(i);let r=OE(40);return iu(e.isScreenSmall()&&r.close())}),pi(75,`mat-icon`,21),eD(76,`inventory_2`),Mc(),pi(77,`span`,22),eD(78,`Items`),Mc()(),pi(79,`a`,31),wp(`click`,function(){ou(i);let r=OE(40);return iu(e.isScreenSmall()&&r.close())}),pi(80,`mat-icon`,21),eD(81,`qr_code_2`),Mc(),pi(82,`span`,22),eD(83,`Payment Settings`),Mc()()()(),pi(84,`mat-sidenav-content`,32)(85,`main`,33),mp(86,`router-outlet`),Mc()()()}if(t&2){let i,a=OE(21);av(15),gp(`matBadge`,e.cartCount())(`matBadgeHidden`,e.cartCount()===0),av(2),gp(`matMenuTriggerFor`,a),av(5),sE((i=e.auth.currentUser())?22:-1,i),av(17),gp(`opened`,!e.isScreenSmall())(`mode`,e.isScreenSmall()?`over`:`side`),av(13),gp(`routerLinkActiveOptions`,dD(9,na)),av(15),gp(`canRender`,`section-users-view`),av(),sE(e.auth.isAdmin()?68:-1)}},dependencies:[Tt,bo,ur,be,fe,Vi,Xe,ji,ve,Se,ke,we,Qt,Ht,oe,ne,Ks,Ui,Ne,mt,vt,Ce,Yt,mt$1,G$1,F,Te],styles:[`[_nghost-%COMP%]{display:block}.app-toolbar[_ngcontent-%COMP%]{position:fixed;top:0;left:0;right:0;z-index:1000}.app-toolbar[_ngcontent-%COMP%]   .brand[_ngcontent-%COMP%]{display:inline-flex;align-items:center;gap:.5rem;font-weight:600;font-size:1.1rem;margin-left:.5rem}.app-toolbar[_ngcontent-%COMP%]   .brand[_ngcontent-%COMP%]   .brand-logo[_ngcontent-%COMP%]{height:46px;width:auto;max-width:160px;object-fit:contain;vertical-align:middle;display:inline-block}.app-toolbar[_ngcontent-%COMP%]   .brand[_ngcontent-%COMP%]   .brand-text[_ngcontent-%COMP%]{white-space:nowrap}.app-toolbar[_ngcontent-%COMP%]   .flex-spacer[_ngcontent-%COMP%]{flex:1 1 auto}.app-sidenav-container[_ngcontent-%COMP%]{position:fixed;inset:64px 0 0}.app-sidenav[_ngcontent-%COMP%]{width:240px}.app-sidenav[_ngcontent-%COMP%]   .link-active[_ngcontent-%COMP%]{background-color:#1976d21f}.app-sidenav[_ngcontent-%COMP%]   .link-active[_ngcontent-%COMP%]     .mdc-list-item__primary-text{font-weight:600}.app-sidenav-content[_ngcontent-%COMP%]{background-color:#f4f6fb}.content[_ngcontent-%COMP%]{min-height:100%}`]})};var $i=n=>({exact:n});var Ji=(n,s)=>s.path;function sa(n,s){if(n&1&&(pi(0,`a`,13)(1,`mat-icon`),eD(2),Mc(),eD(3),Mc()),n&2){let t=s.$implicit;gp(`routerLink`,t.path)(`routerLinkActiveOptions`,fD(4,$i,t.exact)),av(2),Hp(t.icon),av(),xc(` `,t.label,` `)}}function la(n,s){if(n&1&&(pi(0,`nav`,7),lE(1,sa,4,6,`a`,13,Ji),Mc()),n&2){let t=CE();av(),uE(t.navLinks())}}function ca(n,s){if(n&1&&(pi(0,`div`,18)(1,`div`,19),eD(2),Mc(),pi(3,`div`,20),eD(4),Mc()(),mp(5,`mat-divider`)),n&2){let t=s;av(2),Bp(``,t.firstName,` `,t.lastName),av(2),Hp(t.email)}}function ma(n,s){n&1&&(pi(0,`button`,15)(1,`mat-icon`),eD(2,`receipt_long`),Mc(),pi(3,`span`),eD(4,`My Orders`),Mc()())}function da(n,s){if(n&1){let t=mE();pi(0,`button`,14)(1,`mat-icon`),eD(2,`account_circle`),Mc()(),pi(3,`mat-menu`,null,2),iE(5,ca,6,3),iE(6,ma,5,0,`button`,15),pi(7,`button`,16)(8,`mat-icon`),eD(9,`person`),Mc(),pi(10,`span`),eD(11,`My Profile`),Mc()(),pi(12,`button`,17),wp(`click`,function(){ou(t);return iu(CE().logout())}),pi(13,`mat-icon`),eD(14,`logout`),Mc(),pi(15,`span`),eD(16,`Logout`),Mc()()()}if(n&2){let t,e=OE(4),i=CE();gp(`matMenuTriggerFor`,e),av(5),sE((t=i.auth.currentUser())?5:-1,t),av(),sE(i.isCompact()?6:-1)}}function ha(n,s){n&1&&(pi(0,`a`,21)(1,`mat-icon`),eD(2,`login`),Mc(),pi(3,`span`),eD(4,`Login`),Mc()(),pi(5,`a`,22),eD(6,` Sign Up `),Mc())}function pa(n,s){if(n&1&&(pi(0,`a`,23)(1,`mat-icon`),eD(2),Mc(),pi(3,`span`),eD(4),Mc()()),n&2){let t=s.$implicit;gp(`routerLink`,t.path)(`routerLinkActiveOptions`,fD(4,$i,t.exact)),av(2),Hp(t.icon),av(2),Hp(t.label)}}function ua(n,s){if(n&1&&(pi(0,`nav`,11),lE(1,pa,5,6,`a`,23,Ji),Mc()),n&2){let t=CE();av(),uE(t.navLinks())}}var Oe=class n{auth=w(m);cartService=w(p);breakpointObserver=w(ge$1);cartCount=this.cartService.totalItemsCount;isCompact=De(this.breakpointObserver.observe([`(max-width: 900px)`]).pipe(Fe(s=>s.matches)),{initialValue:!1});navLinks=TD(()=>{let s=[{path:`/catalog`,label:`Catalog`,icon:`storefront`,exact:!0}];return this.auth.isAuthenticated()&&s.push({path:`/orders`,label:`My Orders`,icon:`receipt_long`,exact:!1}),s});logout(){this.auth.logout()}static ɵfac=function(t){return new(t||n)};static ɵcmp=FI({type:n,selectors:[[`app-user-shell`]],decls:17,vars:7,consts:[[`brandImg`,``],[`brandFallback`,``],[`userMenu`,`matMenu`],[`color`,`primary`,1,`app-toolbar`,`mat-elevation-z4`],[`routerLink`,`/catalog`,1,`brand`],[`src`,`logo.png`,`alt`,`SRIVIDIKA`,1,`brand-logo`,3,`error`],[1,`brand-text`,`d-none`],[1,`top-nav`],[1,`flex-spacer`],[`mat-icon-button`,``,`routerLink`,`/cart`,`matTooltip`,`Shopping Cart`,`aria-label`,`Shopping Cart`,1,`cart-btn`],[`matBadgeColor`,`warn`,3,`matBadge`,`matBadgeHidden`],[1,`mobile-tab-bar`,`mat-elevation-z2`],[1,`user-content`],[`mat-button`,``,`routerLinkActive`,`link-active`,3,`routerLink`,`routerLinkActiveOptions`],[`mat-icon-button`,``,`aria-label`,`Account`,3,`matMenuTriggerFor`],[`mat-menu-item`,``,`routerLink`,`/orders`],[`mat-menu-item`,``,`routerLink`,`/profile`],[`mat-menu-item`,``,3,`click`],[1,`px-3`,`py-2`],[1,`fw-semibold`],[1,`text-muted`,`small`],[`mat-button`,``,`routerLink`,`/login`,1,`auth-link`],[`mat-flat-button`,``,`color`,`accent`,`routerLink`,`/register`,1,`auth-link`],[`routerLinkActive`,`link-active`,3,`routerLink`,`routerLinkActiveOptions`]],template:function(t,e){if(t&1){let i=mE();pi(0,`mat-toolbar`,3)(1,`a`,4)(2,`img`,5,0),wp(`error`,function(){ou(i);let r=OE(3),d=OE(5);return r.classList.add(`d-none`),iu(d.classList.remove(`d-none`))}),Mc(),pi(4,`span`,6,1),eD(6,`SRIVIDIKA`),Mc()(),iE(7,la,3,0,`nav`,7),mp(8,`span`,8),pi(9,`a`,9)(10,`mat-icon`,10),eD(11,` shopping_cart `),Mc()(),iE(12,da,17,3)(13,ha,7,0),Mc(),iE(14,ua,3,0,`nav`,11),pi(15,`main`,12),mp(16,`router-outlet`),Mc()}t&2&&(av(7),sE(e.isCompact()?-1:7),av(3),gp(`matBadge`,e.cartCount())(`matBadgeHidden`,e.cartCount()===0),av(2),sE(e.auth.isAuthenticated()?12:13),av(2),sE(e.isCompact()&&e.navLinks().length>1?14:-1),av(),Rp(`with-tab-bar`,e.isCompact()&&e.navLinks().length>1))},dependencies:[Tt,bo,ur,be,fe,oe,ne,Ks,Xs,Ui,Ne,mt,vt,Ce,Yt,mt$1,G$1,F,xe,Ze],styles:[`[_nghost-%COMP%]{display:block}.app-toolbar[_ngcontent-%COMP%]{position:fixed;top:0;left:0;right:0;z-index:1000}.app-toolbar[_ngcontent-%COMP%]   .brand[_ngcontent-%COMP%]{display:inline-flex;align-items:center;gap:.5rem;font-weight:700;font-size:1.15rem;margin-left:.25rem;color:inherit;text-decoration:none;letter-spacing:.2px}.app-toolbar[_ngcontent-%COMP%]   .brand[_ngcontent-%COMP%]   .brand-logo[_ngcontent-%COMP%]{height:46px;width:auto;max-width:160px;object-fit:contain;vertical-align:middle;display:inline-block}.app-toolbar[_ngcontent-%COMP%]   .brand[_ngcontent-%COMP%]   .brand-text[_ngcontent-%COMP%]{white-space:nowrap}.app-toolbar[_ngcontent-%COMP%]   .top-nav[_ngcontent-%COMP%]{display:flex;align-items:center;gap:.25rem;margin-left:2rem}.app-toolbar[_ngcontent-%COMP%]   .top-nav[_ngcontent-%COMP%]   a[_ngcontent-%COMP%]{display:inline-flex;align-items:center;gap:.4rem;border-radius:999px;padding-inline:.9rem;opacity:.9;transition:background-color .15s ease,opacity .15s ease}.app-toolbar[_ngcontent-%COMP%]   .top-nav[_ngcontent-%COMP%]   a[_ngcontent-%COMP%]:hover{opacity:1;background-color:#ffffff1f}.app-toolbar[_ngcontent-%COMP%]   .top-nav[_ngcontent-%COMP%]   .link-active[_ngcontent-%COMP%]{opacity:1;background-color:#ffffff2e;font-weight:600}.app-toolbar[_ngcontent-%COMP%]   .flex-spacer[_ngcontent-%COMP%]{flex:1 1 auto}.app-toolbar[_ngcontent-%COMP%]   .cart-btn[_ngcontent-%COMP%]{margin-right:.15rem}.app-toolbar[_ngcontent-%COMP%]   .auth-link[_ngcontent-%COMP%]{margin-left:.4rem;display:inline-flex;align-items:center;gap:.25rem}.user-content[_ngcontent-%COMP%]{position:fixed;inset:64px 0 0;overflow-y:auto;background-color:#f4f6fb}.user-content.with-tab-bar[_ngcontent-%COMP%]{bottom:60px}.mobile-tab-bar[_ngcontent-%COMP%]{position:fixed;left:0;right:0;bottom:0;z-index:999;display:flex;background:#fff}.mobile-tab-bar[_ngcontent-%COMP%]   a[_ngcontent-%COMP%]{flex:1 1 0;display:flex;flex-direction:column;align-items:center;justify-content:center;gap:.15rem;padding:.4rem 0;color:#0009;text-decoration:none;font-size:.7rem}.mobile-tab-bar[_ngcontent-%COMP%]   a[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%]{font-size:1.35rem;height:1.35rem;width:1.35rem}.mobile-tab-bar[_ngcontent-%COMP%]   a.link-active[_ngcontent-%COMP%]{color:#1976d2;font-weight:600}@media screen and (max-width:767px){.user-content[_ngcontent-%COMP%]{padding:0}}`]})};function _a(n,s){n&1&&mp(0,`app-admin-shell`)}function ga(n,s){n&1&&mp(0,`app-user-shell`)}var tn=class n{auth=w(m);showAdminLayout=TD(()=>this.auth.isAdmin()||this.auth.isManager());static ɵfac=function(t){return new(t||n)};static ɵcmp=FI({type:n,selectors:[[`app-shell`]],decls:2,vars:1,template:function(t,e){t&1&&iE(0,_a,1,0,`app-admin-shell`)(1,ga,1,0,`app-user-shell`),t&2&&sE(e.showAdminLayout()?0:1)},dependencies:[Ie,Oe],encapsulation:2})};export{tn as Shell};
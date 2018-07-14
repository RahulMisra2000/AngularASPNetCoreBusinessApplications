import { Injectable } from '@angular/core';
import { UserManager, User } from 'oidc-client';
import { environment } from '../../environments/environment';
import { ReplaySubject } from 'rxjs/ReplaySubject';

@Injectable()
export class OpenIdConnectService {

  private userManager: UserManager = new UserManager(environment.openIdConnectSettings);
  private currentUser: User;


  userLoaded$ = new ReplaySubject<boolean>(1);

  get userAvailable(): boolean {
    return this.currentUser != null;
  }

  get user(): User {
    return this.currentUser;
  }

  constructor() {
    this.userManager.clearStaleState();

    // ************ Event handler we can write for when a user is signed in with IdentityServer4
    this.userManager.events.addUserLoaded(user => {
      if (!environment.production) {
        console.log('User loaded.', user);
      }
      this.currentUser = user;
      this.userLoaded$.next(true);
    });

    // ************ Event handler we can write for when a user is signed out with IdentityServer4
    this.userManager.events.addUserUnloaded((e) => {
      if (!environment.production) {
        console.log('User unloaded');
      }
      this.currentUser = null;
      this.userLoaded$.next(false);
    });

  }

  // ************ Calls the IdentityServer4 to do authentication ... 
  triggerSignIn() {
    this.userManager.signinRedirect().then(function () {
      if (!environment.production) {
        console.log('Redirection to signin triggered.');
      }
    });
  }

  // ************ This method will parse the url for the hash fragment and take some of that content and create
  //              a user object that it will stuff in Session Storage
  //              The user is now signed in with the IdentityServer and even if I delete the SessionStorage and refresh the
  //              Angular application, the user object gets recreated because behind the scene 
  handleCallback() {
    this.userManager.signinRedirectCallback().then(function (user) {
      if (!environment.production) {
        console.log('Callback after signin handled.', user);
      }
    });
  }

  
  handleSilentCallback() {
    this.userManager.signinSilentCallback().then(function (user) {
      this.currentUser = user
      if (!environment.production) {
        console.log('Callback after silent signin handled.', user);
      }
    });
  }

  // ************* When WE call this method, signoutRedirect() will call the ENDSESSION Endpoint at IdentityServer4 
  //               and sign the user out with IdentityServer4 and then delete the Session Storage as well
  triggerSignOut() {
    this.userManager.signoutRedirect().then(function (resp) {
      if (!environment.production) {
        console.log('Redirection to sign out triggered.', resp);
      }
    });
  };
}

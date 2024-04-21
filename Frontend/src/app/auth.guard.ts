import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';


@Injectable({
  providedIn: 'root',
})
export class AuthGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) { }


  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    if (this.authService.isLoggedIn() && this.canActivateRole(route)) {
      return true;
    } else {
      this.router.navigate(['/login']);
      return false;
    }
  }

  private canActivateRole(route: ActivatedRouteSnapshot): boolean {
    const roles = (route.data as { roles: string[] })['roles'];
    const userRole = this.authService.userRole();
    return !roles || roles.length === 0 || roles.includes(userRole);
  }
}
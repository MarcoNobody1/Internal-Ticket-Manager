import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { DrawerModule } from 'primeng/drawer';
import { TagModule } from 'primeng/tag';
import { ToolbarModule } from 'primeng/toolbar';

import { AuthService } from '../../core/auth/auth.service';
import { getUserRoleSeverity } from '../../shared/ui/ui-severity.utils';
import { ThemeToggleComponent } from '../../shared/ui/theme-toggle.component';

interface WorkspaceNavItem {
  label: string;
  route: string;
  icon: string;
  adminOnly?: boolean;
}

@Component({
  selector: 'itm-workspace-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    RouterLinkActive,
    RouterOutlet,
    AvatarModule,
    ButtonModule,
    CardModule,
    DividerModule,
    DrawerModule,
    TagModule,
    ToolbarModule,
    ThemeToggleComponent
  ],
  templateUrl: './workspace-page.component.html',
  styleUrls: ['./workspace-page.component.css']
})
export class WorkspacePageComponent {
  readonly session$ = this.authService.session$;
  readonly navItems: WorkspaceNavItem[] = [
    { label: 'Projects', route: '/workspace/projects', icon: 'pi pi-briefcase' },
    { label: 'Tickets', route: '/workspace/tickets', icon: 'pi pi-ticket' },
    { label: 'Users', route: '/workspace/users', icon: 'pi pi-users', adminOnly: true }
  ];

  isNavCollapsed = false;
  isMobileDrawerOpen = false;

  constructor(private readonly authService: AuthService, private readonly router: Router) {}

  getVisibleNavItems(role: string): WorkspaceNavItem[] {
    return this.navItems.filter((item) => !item.adminOnly || role === 'Admin');
  }

  getRoleSeverity(role: string) {
    return getUserRoleSeverity((role === 'Admin' ? 'Admin' : 'Developer'));
  }

  toggleNavCollapse(): void {
    this.isNavCollapsed = !this.isNavCollapsed;
  }

  openMobileDrawer(): void {
    this.isMobileDrawerOpen = true;
  }

  closeMobileDrawer(): void {
    this.isMobileDrawerOpen = false;
  }

  logout(): void {
    this.closeMobileDrawer();
    this.authService.logout();
    void this.router.navigate(['/login'], {
      queryParams: {
        reason: 'signedOut'
      }
    });
  }
}

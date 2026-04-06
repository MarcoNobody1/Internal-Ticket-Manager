import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';

@Component({
  selector: 'itm-access-denied-page',
  standalone: true,
  imports: [CommonModule, RouterLink, ButtonModule, CardModule],
  templateUrl: './access-denied-page.component.html',
  styleUrls: ['./access-denied-page.component.css']
})
export class AccessDeniedPageComponent {
  readonly blockedRoute = this.activatedRoute.snapshot.queryParamMap.get('from');

  constructor(private readonly activatedRoute: ActivatedRoute) {}
}

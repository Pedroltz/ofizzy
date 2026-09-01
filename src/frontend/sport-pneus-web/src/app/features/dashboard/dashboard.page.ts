import { ChangeDetectionStrategy, Component } from '@angular/core';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';

@Component({ selector: 'app-dashboard-page', imports: [CardModule, TagModule], templateUrl: './dashboard.page.html', changeDetection: ChangeDetectionStrategy.OnPush })
export class DashboardPage {}

import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-list-card-skeleton',
  imports: [],
  templateUrl: './list-card-skeleton.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ListCardSkeletonComponent {}

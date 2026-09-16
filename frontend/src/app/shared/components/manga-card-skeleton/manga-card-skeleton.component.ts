import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-manga-card-skeleton',
  imports: [],
  templateUrl: './manga-card-skeleton.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MangaCardSkeletonComponent {}

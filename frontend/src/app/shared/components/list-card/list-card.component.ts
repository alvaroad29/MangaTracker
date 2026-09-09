import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { PublicMangaListDto } from '../../../core/models/lists.models';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-list-card',
  imports: [RouterLink],
  templateUrl: './list-card.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ListCardComponent {
   list = input.required<PublicMangaListDto>();
  showFollowButton = input(true);
  follow = output<void>();
}

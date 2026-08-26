import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-my-list',
  imports: [],
  templateUrl: './my-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MyListComponent {
  
}

import { Component, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';

import { environment } from '../../../../environments/environment';
import { DistrictService } from '../../../services/district.service';

@Component({
  selector: 'app-landing',
  imports: [RouterLink],
  templateUrl: './landing.html',
  styleUrl: './landing.scss',
})
export class Landing {
  private districtService = inject(DistrictService);

  district = rxResource({
    stream: () => this.districtService.getDistrict(environment.districtId),
  });
}
import { Service, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map } from 'rxjs/operators';

import { environment } from '../../environments/environment';
import {
  CreateDistrictRequest,
  CreateEmployeeRequest,
  DistrictDetail,
  DistrictSummary,
  EmployeeRow,
  MinisterRow,
  OfficeExecutives,
  PagedResponse,
  UpdateDistrictRequest,
  UpdateEmployeeRequest,
} from '../models/district';
import { Department, DepartmentEmployee } from '../models/department';
import {
  UpsertDepartmentRequest,
  AddDepartmentEmployeeRequest,
} from '../models/department';
import { UserAccount, RoleChangeRequest } from '../models/account';

/**
 * Central client for the district domain.
 * All URLs build on environment.apiUrl (dev: '/api' via the Angular proxy -> :5171).
 * Per the lab pattern this service is the only place that knows how to talk to the
 * district API; stores/components consume typed methods and never touch HttpClient.
 */
@Service()
export class DistrictService {
  private readonly http = inject(HttpClient);
  private readonly districtBase = `${environment.apiUrl}/districts`;

  private paging(page: number, pageSize: number): HttpParams {
    return new HttpParams()
      .set('page', String(page))
      .set('pageSize', String(pageSize));
  }

  // ---- District record -------------------------------------------------

  getAll(page = 1, pageSize = 50) {
    return this.http
      .get<PagedResponse<DistrictSummary>>(this.districtBase, {
        params: this.paging(page, pageSize),
      })
      .pipe(map((p) => p.items));
  }

  getDistrict(districtId: number) {
    return this.http.get<DistrictDetail>(`${this.districtBase}/${districtId}`);
  }

  createDistrict(body: CreateDistrictRequest) {
    return this.http.post<DistrictDetail>(this.districtBase, body);
  }

  updateDistrict(districtId: number, body: UpdateDistrictRequest) {
    return this.http.put<DistrictDetail>(
      `${this.districtBase}/${districtId}`,
      body,
    );
  }

  deleteDistrict(districtId: number) {
    return this.http.delete<void>(`${this.districtBase}/${districtId}`);
  }

  // ---- District office employees ---------------------------------------

  getEmployees(districtId: number, page = 1, pageSize = 50) {
    return this.http.get<PagedResponse<EmployeeRow>>(
      `${this.districtBase}/${districtId}/employees`,
      { params: this.paging(page, pageSize) },
    );
  }

  getEmployee(districtId: number, employeeId: number) {
    return this.http.get<EmployeeRow>(
      `${this.districtBase}/${districtId}/employees/${employeeId}`,
    );
  }

  createEmployee(districtId: number, body: CreateEmployeeRequest) {
    return this.http.post<EmployeeRow>(
      `${this.districtBase}/${districtId}/employees`,
      body,
    );
  }

  updateEmployee(
    districtId: number,
    employeeId: number,
    body: UpdateEmployeeRequest,
  ) {
    return this.http.put<EmployeeRow>(
      `${this.districtBase}/${districtId}/employees/${employeeId}`,
      body,
    );
  }

  deleteEmployee(districtId: number, employeeId: number) {
    return this.http.delete<void>(
      `${this.districtBase}/${districtId}/employees/${employeeId}`,
    );
  }

  getExecutives(districtId: number) {
    return this.http.get<OfficeExecutives>(
      `${this.districtBase}/${districtId}/employees/executives`,
    );
  }

  // ---- District ministers ---------------------------------------------

  getMinisters(districtId: number, page = 1, pageSize = 50, search?: string) {
    let params = this.paging(page, pageSize);
    if (search) {
      params = params.set('search', search);
    }
    return this.http.get<PagedResponse<MinisterRow>>(
      `${this.districtBase}/${districtId}/ministers`,
      { params },
    );
  }

  // ---- District departments -------------------------------------------

  getDepartments(districtId: number, page = 1, pageSize = 50) {
    return this.http.get<PagedResponse<Department>>(
      `${this.districtBase}/${districtId}/departments`,
      { params: this.paging(page, pageSize) },
    );
  }

  getDepartment(districtId: number, departmentId: number) {
    return this.http.get<Department>(
      `${this.districtBase}/${districtId}/departments/${departmentId}`,
    );
  }

  createDepartment(districtId: number, body: UpsertDepartmentRequest) {
    return this.http.post<Department>(
      `${this.districtBase}/${districtId}/departments`,
      body,
    );
  }

  updateDepartment(
    districtId: number,
    departmentId: number,
    body: UpsertDepartmentRequest,
  ) {
    return this.http.put<Department>(
      `${this.districtBase}/${districtId}/departments/${departmentId}`,
      body,
    );
  }

  deleteDepartment(districtId: number, departmentId: number) {
    return this.http.delete<void>(
      `${this.districtBase}/${districtId}/departments/${departmentId}`,
    );
  }

  getDepartmentEmployees(districtId: number, departmentId: number) {
    return this.http.get<DepartmentEmployee[]>(
      `${this.districtBase}/${districtId}/departments/${departmentId}/employees`,
    );
  }

  assignDepartmentEmployee(
    districtId: number,
    departmentId: number,
    body: AddDepartmentEmployeeRequest,
  ) {
    return this.http.post<DepartmentEmployee>(
      `${this.districtBase}/${districtId}/departments/${departmentId}/employees`,
      body,
    );
  }

  removeDepartmentEmployee(
    districtId: number,
    departmentId: number,
    departmentEmployeeId: number,
  ) {
    return this.http.delete<DepartmentEmployee>(
      `${this.districtBase}/${districtId}/departments/${departmentId}/employees/${departmentEmployeeId}`,
    );
  }

  // ---- District accounts ----------------------------------------------

  getAccounts(districtId: number) {
    return this.http.get<UserAccount[]>(
      `${this.districtBase}/${districtId}/accounts`,
    );
  }

  changeAccountRole(
    districtId: number,
    userId: string,
    body: RoleChangeRequest,
  ) {
    return this.http.post<void>(
      `${this.districtBase}/${districtId}/accounts/${userId}/roles`,
      body,
    );
  }
}

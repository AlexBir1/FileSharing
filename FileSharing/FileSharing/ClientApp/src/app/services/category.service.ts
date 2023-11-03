import { HttpClient } from "@angular/common/http";
import { Inject, Injectable } from "@angular/core";
import { CategoryModel } from "../Entity/CategoryModel";
import { ResponseModel } from "../Entity/ResponseModel";
import { AccountService } from "./account.service";


@Injectable()
export class CategoryService {

  private url: string;

  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrl: string, private accountService: AccountService)
  {
    this.url = baseUrl;
  }

  getCategories() {
    let headers = this.accountService.makeJWTHeader();
    return this.http.get<ResponseModel<CategoryModel[]>>(this.url + 'category/GetCategories', { headers });
  }

  updateCategory(model: CategoryModel) {
    let headers = this.accountService.makeJWTHeader();
    return this.http.patch<ResponseModel<CategoryModel>>(this.url + 'category/UpdateCategory', model, { headers });
  }

  deleteCategory(id: number) {
    let headers = this.accountService.makeJWTHeader();
    return this.http.delete<ResponseModel<CategoryModel[]>>(this.url + 'category/DeleteCategory/' + id, { headers });
  }

  createCategory(model: CategoryModel) {
    let headers = this.accountService.makeJWTHeader();
    return this.http.post<ResponseModel<CategoryModel>>(this.url + 'category/CreateCategory', model, { headers });
  }
}

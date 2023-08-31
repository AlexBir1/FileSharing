import { Inject, Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AccountModel } from './Entity/AccountModel';
import { ResponseModel } from './Entity/ResponseModel';
import { RegisterModel } from './Entity/RegisterModel';
import { LoginModel } from './Entity/LoginModel';
import { environment } from '../environments/environment.prod';
import { map, of, ReplaySubject } from 'rxjs';
import { UpdateAccountModel } from './Entity/UpdateAccountModel';
import { AccountInfoModel } from './Entity/AccountInfoModel';
import { AccountRoleModel } from './Entity/AccountRoleModel';

@Injectable()
export class AccountService {
  private accountSource = new ReplaySubject<AccountModel | null>(1);
  currentAccount$ = this.accountSource.asObservable();


  private url: string;
  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.url = baseUrl;
  }

  refreshAccount() {
    const jwt = this.getJWTToken();
    if (jwt) {
      this.getAndSaveAccount(jwt).subscribe({
        next: () => {
        },
        error: () => {
          this.SignOut();
        }
      })
    }
    else {
      this.getAndSaveAccount(null).subscribe();
    }
  }

  getAndSaveAccount(jwt: string | null) {
    if (jwt === null) {
      this.accountSource.next(null);
      return of(undefined);
    }
    else {
      let headers = new HttpHeaders();
      headers = headers.set('Authorization', 'Bearer ' + jwt);
      return this.http.get<AccountModel>(this.url + 'account/refreshTken', { headers }).pipe(map((account: AccountModel) =>
      {
        if (account) {
          this.saveAccount(account);
        }
      }));
    }
  }

  getAdditionalInfo(accountId: string) {
    let headers = this.makeJWTHeader();
    return this.http.get<ResponseModel<AccountInfoModel>>(this.url + `account/AdditionalInfo/${accountId}`, { headers })
  }

  getAccountId() {
    const key = localStorage.getItem(environment.userKey);
    if (key) {
      const account: AccountModel = JSON.parse(key);
      var accountId = account.id;
      return accountId;
    }
    else {
      return null;
    } 
  }

  makeJWTHeader() {
    let jwt = this.getJWTToken();
    let headers = new HttpHeaders();
    headers = headers.set('Authorization', 'Bearer ' + jwt);
    return headers;
  }

  SignOut() {
    localStorage.removeItem(environment.userKey);
    this.accountSource.next(null);
  }

  getJWTToken() {
    const key = localStorage.getItem(environment.userKey);
    if (key) {
      const account: AccountModel = JSON.parse(key);
      var tken = account.jwtToken;
      return tken;
    }
    else {
      return null;
    }
  }

  SignUp(registerModel: RegisterModel) {
    return this.http.post<ResponseModel<AccountModel>>(this.url + 'account/SignUp', registerModel);
  }

  SignIn(loginModel: LoginModel) {
    return this.http.post<ResponseModel<AccountModel>>(this.url + 'account/SignIn', loginModel);
  }

  saveAccount(accountModel: AccountModel) {
    localStorage.setItem(environment.userKey, JSON.stringify(accountModel));
    this.accountSource.next(accountModel);
  }

  updateAccount(updateModel: UpdateAccountModel) {
    let headers = this.makeJWTHeader();
    return this.http.patch<ResponseModel<AccountModel>>(this.url + 'account/UpdateAccount', updateModel, { headers });
  }

  deleteAccount(id: string) {
    let headers = this.makeJWTHeader();
    return this.http.delete<ResponseModel<AccountModel>>(this.url + 'account/DeleteAccount/' + id, { headers });
  }

  getAccounts() {
    let headers = this.makeJWTHeader();
    return this.http.get<ResponseModel<AccountModel[]>>(this.url + 'account/GetAccounts', { headers });
  }

  getAvailableRoles() {
    return this.http.get<ResponseModel<string[]>>(this.url + 'account/GetAvailableRoles');
  }

  setAccountRole(accountId: string, newRole: string) {
    let headers = this.makeJWTHeader();
    var accountRoleModel = new AccountRoleModel();
    accountRoleModel.accountId = accountId;
    accountRoleModel.role = newRole;
    return this.http.patch<ResponseModel<AccountModel>>(this.url + `account/ChangeRole`, accountRoleModel, { headers });
  }
}

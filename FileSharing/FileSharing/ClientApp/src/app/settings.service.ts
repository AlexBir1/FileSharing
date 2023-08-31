import { HttpClient } from "@angular/common/http";
import { Inject, Injectable } from "@angular/core";
import { ReplaySubject } from "rxjs";
import { environment } from "../environments/environment.prod";
import { AccountService } from "./account.service";
import { ResponseModel } from "./Entity/ResponseModel";
import { SettingsModel } from "./Entity/SettingsModel";

@Injectable()
export class SettingsService {
  url: string;
  private settingsSource = new ReplaySubject<SettingsModel[] | null>(1);
  currentSettings$ = this.settingsSource.asObservable();

  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrl: string, private accountService: AccountService) {
    this.url = baseUrl;
  }

  refreshSettings() {
    this.accountService.currentAccount$.subscribe(x => {
      if (x === null) {
        this.setLocalSettings(null)
      }
      else {
        this.getSettings(x.id).subscribe(x => {
          if (x.isSuccessful) {
            this.setLocalSettings(x.data as SettingsModel[]);
          }
        });
      }
    })
  }

  setLocalSettings(model: SettingsModel[] | null) {
    if (model === null) {
      localStorage.removeItem(environment.settingsProfile);
      this.settingsSource.next(null);
      return;
    }
    localStorage.setItem(environment.settingsProfile, JSON.stringify(model));
    this.settingsSource.next(model);
  }

  getSettings(account_id: string) {
    var headers = this.accountService.makeJWTHeader();
    return this.http.get<ResponseModel<SettingsModel[]>>(this.url + 'settings/GetSettings/' + account_id, { headers });
  }
  setSetting(settings: SettingsModel[]) {
    var headers = this.accountService.makeJWTHeader();
    return this.http.patch<ResponseModel<SettingsModel[]>>(this.url + 'settings/SetAccountSettings/', settings, { headers });
  }

  equalizeSettingsWithDB() {
    return this.http.get<ResponseModel<SettingsModel[]>>(this.url + 'settings/equalizeSettingsFileWithDB/');
  }
}

export class LoginModel {
  public someid: string;
  public password: string;

  constructor(someid: string, password: string) {
    this.someid = someid;
    this.password = password;
  }
}

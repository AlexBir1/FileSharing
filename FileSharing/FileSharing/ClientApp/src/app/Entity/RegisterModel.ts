export class RegisterModel {
  public username: string;
  public email: string;
  public password: string;
  public passwordConfirm: string;

  constructor(Username: string, Email: string, Password: string, PasswordConfirm: string) {
    this.username = Username;
    this.email = Email;
    this.password = Password;
    this.passwordConfirm = PasswordConfirm;
  }
}

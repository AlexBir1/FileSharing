import { AccountModel } from "../Entity/AccountModel";


export class AccountHandler {
  public currentAccount!: AccountModel;
  public currentAccountRole!: string;
  public currentAccountRegDate!: string;

  public selectedAccount!: AccountModel | null;
  public selectedAccountRole!: string;
  public selectedAccountRegDate!: string;

  public accountList: AccountModel[] = [];
  public filteredAccountList: AccountModel[] = [];

  public currentDeleteId!: string;
  public selectedDeleteId!: string;

  public getAccountArray() {
    return this.accountList;
  }

  public setAccountArrayFiltered(partOfUsernameOrEmail: string) {
    this.accountList.forEach(x => {
      if ((x.username.toLowerCase().includes(partOfUsernameOrEmail.toLowerCase()) || x.email.toLowerCase().includes(partOfUsernameOrEmail.toLowerCase())))
        if (this.filteredAccountList.length <= this.accountList.length && this.filteredAccountList.find(y => y.id == x.id) === undefined)
          this.filteredAccountList.push(x);
    })
  }

  getAccountById(id: string) {
    return this.accountList.find(x => x.id == id) as AccountModel;
  }

  setCurrentAccount(account: AccountModel) {
    this.currentAccount = account;
    let dateToStr = new Date(account.registrationDate);
    this.currentAccountRegDate = dateToStr.toLocaleDateString("en-US");
    this.currentDeleteId = account.id;
    this.currentAccountRole = account.roles[0];
  }

  setSelectedAccount(account: AccountModel | null) {
    if (account === null) {
      this.selectedAccount = null;
    }
    else {
      this.selectedAccount = account;
      let dateToStr = new Date(account.registrationDate);
      this.selectedAccountRegDate = dateToStr.toLocaleDateString("en-US");
      this.selectedDeleteId = account.id;
      this.selectedAccountRole = account.roles[0];
    }
  }

  setAccountArray(accounts: AccountModel[]) {
    this.accountList = accounts;
  }

  getCurrentAccount() {
    return this.currentAccount;
  }
}

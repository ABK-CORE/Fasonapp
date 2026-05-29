export class LoginCredentials {
  constructor(public username: string = '', public password: string = '') {}
}

export interface TokenUser {
  UserId: number;
  Username: string;
  CompanyName: string;
  Roles?: string[];
}

export interface TokenData {
  Token: string;
  Expiration: string;     // ISO
  User: TokenUser;
  IsSuccessful: boolean;
  IsVerified: boolean;
}

export interface LoginApiResponse {
  data: TokenData;
  success: boolean;
  message: string;
}

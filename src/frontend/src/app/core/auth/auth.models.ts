export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresAtUtc: string;
  username: string;
  role: string;
}

export interface AuthSession {
  accessToken: string;
  expiresAtUtc: string;
  username: string;
  role: string;
}

export type AuthRedirectReason = 'authenticationRequired' | 'sessionExpired' | 'signedOut';

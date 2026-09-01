export interface CurrentUser { id: string; name: string; email: string; }
export interface SetupRequest { companyName: string; cnpj: string | null; phone: string | null; adminName: string; email: string; password: string; }

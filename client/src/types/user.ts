export type User = {
    id: string;
    displayName: string;
    email: string;
    token: string;
    imageUrl?: string;
    roles: string[];
}

export type LoginCreds = {
    email: string;
    password: string;
}

export type RegisterCreds = {
    email: string;
    displayName: string;
    password: string;
    gender: string;
    dateOfBirth: string;
    city: string;
    country: string;
}
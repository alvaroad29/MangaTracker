import { MangaEntryDto } from "./entries.models";
import { ReadingStatus } from "./enums";

export interface MangaListDto {
  id: string;
  name: string;
  description: string | null;
  isPublic: boolean;
  itemCount: number;
  ownerId: string;
  createdAt: string;
}

export interface MangaListItemDto {
  id: string;
  entry: MangaEntryDto;
  addedAt: string;
}

export interface PublicMangaListDto {
  id: string;
  name: string;
  description: string | null;
  itemCount: number;
  followerCount: number;
  ownerId: string;
  ownerDisplayName: string;
  ownerAvatarUrl: string | null;
  isFollowedByMe: boolean;
  createdAt: string;
}



export interface CreateMangaListRequest {
  name: string;
  description: string | null;
  isPublic: boolean;
}

export interface MangaListDetailDto {
  id: string;
  name: string;
  description: string | null;
  isPublic: boolean;
  ownerId: string;
  ownerDisplayName: string;
  ownerAvatarUrl: string | null;
  followerCount: number;
  isFollowedByMe: boolean;
  createdAt: string;
  items: MangaListItemDto[];
}

export interface PublicUserProfileDto {
  id: string;
  displayName: string;
  avatarUrl: string | null;
}

export interface AddMangaToListRequest {
  malId: number;
  defaultStatus: ReadingStatus;
}

export type UpdateMangaListRequest = CreateMangaListRequest;

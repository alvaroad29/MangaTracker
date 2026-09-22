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

export interface PublicMangaListDto {
  id: string;
  name: string;
  description: string | null;
  mangaCount: number;
  followerCount: number;
  ownerId: string;
  ownerDisplayName: string;
  ownerAvatarUrl: string | null;
  isFollowedByMe: boolean;
  createdAt: string;
}

export interface PublicListSearchResponse {
  items: PublicMangaListDto[];
  currentPage: number;
  hasNextPage: boolean;
  lastVisiblePage: number;
  totalItemsCount: number;
}
export interface MangaListItemDto {
  id: string;
  entry: MangaEntryDto;
  addedAt: string;
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
  itemsCurrentPage: number;
  itemsHasNextPage: boolean;
  itemsLastVisiblePage: number;
  itemsTotalCount: number;
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

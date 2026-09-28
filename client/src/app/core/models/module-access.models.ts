export interface SectionDto {
  id: string;
  pageId: string;
  name: string;
  key: string;
  sortOrder: number;
  isActive: boolean;
}

export interface PageDto {
  id: string;
  moduleId: string;
  name: string;
  url: string;
  sortOrder: number;
  isActive: boolean;
  sections: SectionDto[];
}

export interface ModuleDto {
  id: string;
  name: string;
  key: string;
  sortOrder: number;
  isActive: boolean;
  pages: PageDto[];
}

export interface RoleAccessDto {
  roleId: string;
  roleName: string;
  sectionKeys: string[];
}

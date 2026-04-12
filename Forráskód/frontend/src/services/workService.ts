import { isAxiosError } from 'axios'
import api from './axios'
import type {
  Work,
  WorkApplicant,
  WorkTodo,
  WorkImage,
  FeaturedWork,
  PaginatedWorks,
  WorkFilters,
  WorkImageMetadataUpdate,
} from '@/types/work'

// Work API szolgáltatás:
// a teljes munka-modul kliens oldali végponttérképe egy helyen.
// A komponensek ezeken a metódusokon keresztül hívják a backendet,
// így a route/view rétegek mentesülnek az URL építés és kompatibilitás logika alól.

const API_URL = '/Work'

export const workService = {
  // -----------------------------
  // LISTÁZÓ VÉGPONTOK
  // -----------------------------
  // "visible": user feed jellegű lista (nyitott + saját + releváns munkák).
  getVisibleWorks(page: number = 1, pageSize: number = 6, filters?: WorkFilters) {
    let url = `${API_URL}/visible?page=${page}&pageSize=${pageSize}`;
    if (filters) {
      if (filters.priceMin !== undefined && filters.priceMin !== null) {
        url += `&priceMin=${filters.priceMin}`;
      }
      if (filters.priceMax !== undefined && filters.priceMax !== null) {
        url += `&priceMax=${filters.priceMax}`;
      }
      if (filters.createdFrom) {
        url += `&createdFrom=${filters.createdFrom}`;
      }
      if (filters.createdTo) {
        url += `&createdTo=${filters.createdTo}`;
      }
      if (filters.targetAudience) {
        url += `&targetAudience=${filters.targetAudience}`;
      }
      if (filters.status && filters.status.length > 0) {
        url += `&status=${filters.status.join(',')}`;
      }
    }
    return api.get<PaginatedWorks>(url);
  },

  // "my": saját munka-nézet.
  getMyWorks(page: number = 1, pageSize: number = 6, filters?: WorkFilters) {
    let url = `${API_URL}/my?page=${page}&pageSize=${pageSize}`;
    if (filters) {
      if (filters.priceMin !== undefined && filters.priceMin !== null) {
        url += `&priceMin=${filters.priceMin}`;
      }
      if (filters.priceMax !== undefined && filters.priceMax !== null) {
        url += `&priceMax=${filters.priceMax}`;
      }
      if (filters.createdFrom) {
        url += `&createdFrom=${filters.createdFrom}`;
      }
      if (filters.createdTo) {
        url += `&createdTo=${filters.createdTo}`;
      }
      if (filters.targetAudience) {
        url += `&targetAudience=${filters.targetAudience}`;
      }
      if (filters.status && filters.status.length > 0) {
        url += `&status=${filters.status.join(',')}`;
      }
    }
    return api.get<PaginatedWorks>(url);
  },

  // "open": publikus/nyitott munkák listája.
  getOpenWorks(page: number = 1, pageSize: number = 6, filters?: WorkFilters) {
    let url = `${API_URL}/open?page=${page}&pageSize=${pageSize}`;
    if (filters) {
      if (filters.priceMin !== undefined && filters.priceMin !== null) {
        url += `&priceMin=${filters.priceMin}`;
      }
      if (filters.priceMax !== undefined && filters.priceMax !== null) {
        url += `&priceMax=${filters.priceMax}`;
      }
      if (filters.createdFrom) {
        url += `&createdFrom=${filters.createdFrom}`;
      }
      if (filters.createdTo) {
        url += `&createdTo=${filters.createdTo}`;
      }
      if (filters.targetAudience) {
        url += `&targetAudience=${filters.targetAudience}`;
      }
      if (filters.status && filters.status.length > 0) {
        url += `&status=${filters.status.join(',')}`;
      }
    }
    return api.get<PaginatedWorks>(url);
  },

  // Admin publikus munkák listája moderáció/kiemelés célra.
  getAdminPublicWorks() {
    return api.get<Work[]>(`${API_URL}/admin/public`);
  },

  // Egy munka teljes részletes adatlapja.
  getWork(id: number) {
    return api.get(`${API_URL}/${id}`);
  },

  // Jelentkezők listája egy munkához.
  getApplicants(id: number) {
    return api.get<WorkApplicant[]>(`${API_URL}/${id}/applicants`);
  },

  // Új munka létrehozása.
  createWork(work: Work) {
    return api.post(`${API_URL}`, work);
  },

  // Jelentkezés munkára ajánlati árral.
  // Kompatibilitás miatt két kérésadat-formátumot is próbálunk:
  // 1) objektum { offeredPrice }
  // 2) primitive decimal
  applyForWork(id: number, offeredPrice: number | null) {
    return api.post(`${API_URL}/${id}/apply`, { offeredPrice }, {
      headers: { 'Content-Type': 'application/json' }
    }).catch((error) => {
      if (isAxiosError(error) && error.response?.status === 400) {
        return api.post(`${API_URL}/${id}/apply`, offeredPrice, {
          headers: { 'Content-Type': 'application/json' }
        });
      }

      throw error;
    });
  },

  // Jelentkező elfogadása.
  acceptApplicant(applicantId: number) {
    return api.post(`${API_URL}/applicant/${applicantId}/accept`);
  },

  // Teendő hozzáadása munkához.
  addTodo(id: number, todo: WorkTodo) {
    return api.post(`${API_URL}/${id}/todo`, todo);
  },

  // Teendő teljesítettre állítása szöveges megjegyzéssel.
  toggleTodo(todoId: number, doneMessage: string) {
    return api.post(`${API_URL}/todo/${todoId}/toggle`, JSON.stringify(doneMessage), {
      headers: { 'Content-Type': 'application/json' }
    });
  },

  // Egy kép feltöltése multipart form-data-ban.
  uploadImage(id: number, file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return api.post(`${API_URL}/${id}/image`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
  },

  // Kiemelt (showcase) flag kapcsolása.
  toggleShowcaseImage(imageId: number) {
    return api.post(`${API_URL}/image/${imageId}/showcase`);
  },

  // Munka státusz frissítése.
  updateStatus(id: number, status: string) {
    return api.put(`${API_URL}/${id}/status`, JSON.stringify(status), {
      headers: { 'Content-Type': 'application/json' }
    });
  },

  // Kiemelt munkák listája.
  getFeaturedWorks() {
    return api.get(`${API_URL}/featured`);
  },

  // Munka kiemelése.
  featureWork(id: number) {
    return api.post(`${API_URL}/${id}/feature`);
  },

  // Kiemelés törlése.
  removeFeaturedWork(id: number) {
    return api.delete(`${API_URL}/featured/${id}`);
  },

  // Munka szerkesztése.
  updateWork(id: number, work: Partial<Work>) {
    return api.put(`${API_URL}/${id}`, work);
  },

  // Munka törlése.
  deleteWork(id: number) {
    return api.delete(`${API_URL}/${id}`);
  },

  // Jelentkező elutasítása.
  rejectApplicant(applicantId: number) {
    return api.post(`${API_URL}/applicant/${applicantId}/reject`);
  },

  // Saját jelentkezés visszavonása.
  withdrawApplication(workId: number, applicantId: number) {
    return api.post(`${API_URL}/applicant/${applicantId}/withdraw`);
  },

  // Egy adott kép törlése munkából.
  deleteImage(workId: number, imageId: number) {
    return api.delete(`${API_URL}/${workId}/image/${imageId}`);
  },

  // Kép metadata frissítése (showcase, kapcsolt kép, stb.).
  updateImageMetadata(workId: number, imageId: number, metadata: WorkImageMetadataUpdate) {
    return api.patch(`${API_URL}/${workId}/image/${imageId}`, metadata);
  },

  // Képpár összekötés before/after nézethez.
  linkImagePair(imageId: number, relatedImageId: number) {
    return api.post(`${API_URL}/image/${imageId}/link`, { relatedImageId });
  },

  // Bulk képfeltöltés ugyanahhoz a munkához.
  uploadImages(workId: number, files: File[]) {
    const formData = new FormData();
    files.forEach((file) => {
      formData.append('files', file);
    });
    return api.post(`${API_URL}/${workId}/images`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
  }
};

// Típusok újraexportja a fogyasztó oldalak kényelmes importjához.
export type { Work, WorkApplicant, WorkTodo, WorkImage, FeaturedWork, PaginatedWorks } from '@/types/work'

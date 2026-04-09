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

// Types exported from @/types/work
// Using the types exported from work.ts instead of defining inline

const API_URL = '/Work'

export const workService = {
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

  getWork(id: number) {
    return api.get(`${API_URL}/${id}`);
  },

  createWork(work: Work) {
    return api.post(`${API_URL}`, work);
  },

  applyForWork(id: number, offeredPrice: number | null) {
    return api.post(`${API_URL}/${id}/apply`, offeredPrice, {
      headers: { 'Content-Type': 'application/json' }
    });
  },

  acceptApplicant(applicantId: number) {
    return api.post(`${API_URL}/applicant/${applicantId}/accept`);
  },

  addTodo(id: number, todo: WorkTodo) {
    return api.post(`${API_URL}/${id}/todo`, todo);
  },

  toggleTodo(todoId: number, doneMessage: string) {
    return api.post(`${API_URL}/todo/${todoId}/toggle`, JSON.stringify(doneMessage), {
      headers: { 'Content-Type': 'application/json' }
    });
  },

  uploadImage(id: number, file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return api.post(`${API_URL}/${id}/image`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
  },

  toggleShowcaseImage(imageId: number) {
    return api.post(`${API_URL}/image/${imageId}/showcase`);
  },

  updateStatus(id: number, status: string) {
    return api.put(`${API_URL}/${id}/status`, JSON.stringify(status), {
      headers: { 'Content-Type': 'application/json' }
    });
  },

  getFeaturedWorks() {
    return api.get(`${API_URL}/featured`);
  },

  featureWork(id: number) {
    return api.post(`${API_URL}/${id}/feature`);
  },

  removeFeaturedWork(id: number) {
    return api.delete(`${API_URL}/featured/${id}`);
  },

  updateWork(id: number, work: Partial<Work>) {
    return api.put(`${API_URL}/${id}`, work);
  },

  deleteWork(id: number) {
    return api.delete(`${API_URL}/${id}`);
  },

  rejectApplicant(applicantId: number) {
    return api.post(`${API_URL}/applicant/${applicantId}/reject`);
  },

  withdrawApplication(workId: number, applicantId: number) {
    return api.post(`${API_URL}/applicant/${applicantId}/withdraw`);
  },

  deleteImage(workId: number, imageId: number) {
    return api.delete(`${API_URL}/${workId}/image/${imageId}`);
  },

  updateImageMetadata(workId: number, imageId: number, metadata: WorkImageMetadataUpdate) {
    return api.patch(`${API_URL}/${workId}/image/${imageId}`, metadata);
  },

  linkImagePair(imageId: number, relatedImageId: number) {
    return api.post(`${API_URL}/image/${imageId}/link`, { relatedImageId });
  },

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

// Re-export types for convenience
export type { Work, WorkApplicant, WorkTodo, WorkImage, FeaturedWork, PaginatedWorks } from '@/types/work'

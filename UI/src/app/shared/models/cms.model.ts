// CMS Models
export interface Page {
    id: string; // GUID
    slug: string;
    titleEn: string;
    titleAr: string;
    metaDescriptionEn?: string;
    metaDescriptionAr?: string;
    contentEn?: string;
    contentAr?: string;
    isActive: boolean;
    isHomePage: boolean;
    sections: PageSection[];
    createdAt: string;
    updatedAt?: string;
}

export interface PageSection {
    id: string; // GUID
    pageId: string;
    type: SectionType;
    order: number;
    titleEn?: string;
    titleAr?: string;
    contentEn?: string;
    contentAr?: string;
    settings?: any; // JSON settings
    createdAt: string;
}

export enum SectionType {
    Hero = 'Hero',
    Banner = 'Banner',
    FeaturedCategories = 'FeaturedCategories',
    FeaturedItems = 'FeaturedItems',
    Testimonials = 'Testimonials',
    Newsletter = 'Newsletter',
    Content = 'Content',
    Custom = 'Custom'
}

export interface HomePageContent {
    hero: HeroSection;
    banners: Banner[];
    featuredCategories: FeaturedCategory[];
    featuredItems: FeaturedItem[];
    testimonials?: Testimonial[];
    newsletter?: NewsletterSection;
}

export interface HeroSection {
    titleEn: string;
    titleAr: string;
    subtitleEn?: string;
    subtitleAr?: string;
    imageUrl?: string;
    videoUrl?: string;
    ctaTextEn?: string;
    ctaTextAr?: string;
    ctaLink?: string;
    overlayColor?: string;
}

export interface Banner {
    id: string; // GUID
    titleEn?: string;
    titleAr?: string;
    imageUrl: string;
    linkUrl?: string;
    order: number;
    isActive: boolean;
    startDate?: string;
    endDate?: string;
}

export interface FeaturedCategory {
    categoryId: string;
    categoryNameEn: string;
    categoryNameAr: string;
    imageUrl?: string;
    linkUrl: string;
    order: number;
}

export interface FeaturedItem {
    itemId: string;
    itemNameEn: string;
    itemNameAr: string;
    imageUrl?: string;
    linkUrl: string;
    price: number;
    currency: string;
    order: number;
}

export interface Testimonial {
    id: string; // GUID
    nameEn: string;
    nameAr: string;
    textEn: string;
    textAr: string;
    rating: number;
    imageUrl?: string;
    company?: string;
    order: number;
    isActive: boolean;
}

export interface NewsletterSection {
    titleEn: string;
    titleAr: string;
    descriptionEn?: string;
    descriptionAr?: string;
    isActive: boolean;
}

export interface CreatePageRequest {
    slug: string;
    titleEn: string;
    titleAr: string;
    metaDescriptionEn?: string;
    metaDescriptionAr?: string;
    contentEn?: string;
    contentAr?: string;
    isActive: boolean;
    isHomePage?: boolean;
}

export interface UpdatePageRequest {
    titleEn?: string;
    titleAr?: string;
    metaDescriptionEn?: string;
    metaDescriptionAr?: string;
    contentEn?: string;
    contentAr?: string;
    isActive?: boolean;
}

export interface CreatePageSectionRequest {
    type: SectionType;
    order: number;
    titleEn?: string;
    titleAr?: string;
    contentEn?: string;
    contentAr?: string;
    settings?: any;
}


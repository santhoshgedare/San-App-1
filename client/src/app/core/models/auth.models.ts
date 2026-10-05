export interface UserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
  createdAt: string;
  lastLoginAt: string | null;
  roles: string[];
}

export interface RoleDto {
  id: string;
  name: string;
  userCount: number;
}

export enum UnitOfMeasurement {
  Piece = 'Piece',
  Kg = 'Kg',
  Gram = 'Gram',
  Litre = 'Litre',
  Millilitre = 'Millilitre',
  Meter = 'Meter',
  Centimeter = 'Centimeter',
  Box = 'Box',
  Pack = 'Pack',
  Set = 'Set',
}

export enum CategoryVariantType {
  Color = 'Color',
  Size = 'Size',
  Material = 'Material',
  Style = 'Style',
  Pattern = 'Pattern',
  Weight = 'Weight',
  Texture = 'Texture',
  Custom = 'Custom',
}

export interface CategoryVariantDefinition {
  id: string;
  name: string;
  type: CategoryVariantType;
  values: string[];
  isRequired: boolean;
}

export interface CategoryDto {
  id: string;
  name: string;
  description: string | null;
  isActive: boolean;
  unitOfMeasurement: UnitOfMeasurement;
  variantDefinitions: CategoryVariantDefinition[];
  createdAt?: string;
}

export interface CreateCategoryRequest {
  name: string;
  description?: string | null;
  isActive?: boolean;
  unitOfMeasurement?: UnitOfMeasurement;
  variantDefinitions?: CategoryVariantDefinition[];
}

export interface UpdateCategoryRequest {
  name: string;
  description?: string | null;
  isActive?: boolean;
  unitOfMeasurement?: UnitOfMeasurement;
  variantDefinitions?: CategoryVariantDefinition[];
}

export interface ItemImageDto {
  id?: string;
  itemId?: string;
  url: string;
  fileName?: string | null;
  caption?: string | null;
  isPrimary: boolean;
  sortOrder: number;
  uploadedAt?: string;
}

export interface ItemDocumentDto {
  id?: string;
  itemId?: string;
  url: string;
  fileName: string;
  documentType?: string | null;
  fileSizeBytes: number;
  description?: string | null;
  uploadedAt?: string;
}

export interface ItemVariantDto {
  id?: string;
  itemId?: string;
  sku: string;
  name?: string;
  barcode?: string | null;
  attributesJson: string;
  price: number;
  costPrice: number;
  stockQuantity: number;
  isActive: boolean;
}

export interface ItemDto {
  id: string;
  code: string;
  name: string;
  description: string | null;
  barcode: string | null;
  categoryId: string;
  categoryName: string;
  unitOfMeasurement: UnitOfMeasurement;
  price: number;
  costPrice: number;
  stockQuantity: number;
  isActive: boolean;
  createdAt?: string;
  images: ItemImageDto[];
  documents: ItemDocumentDto[];
  variants: ItemVariantDto[];
}

export interface CreateItemRequest {
  code: string;
  name: string;
  description?: string | null;
  barcode?: string | null;
  categoryId: string;
  unitOfMeasurement?: string;
  price: number;
  costPrice: number;
  stockQuantity: number;
  isActive: boolean;
  images?: ItemImageDto[];
  documents?: ItemDocumentDto[];
  variants?: ItemVariantDto[];
}

export interface UpdateItemRequest {
  code: string;
  name: string;
  description?: string | null;
  barcode?: string | null;
  categoryId: string;
  unitOfMeasurement?: string;
  price: number;
  costPrice: number;
  stockQuantity: number;
  isActive: boolean;
  images?: ItemImageDto[];
  documents?: ItemDocumentDto[];
  variants?: ItemVariantDto[];
}

export interface AuthResult {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserDto;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface ApiError {
  errors: string[];
}

export enum OrderStatus {
  Pending = 'Pending',
  Confirmed = 'Confirmed',
  Processing = 'Processing',
  Shipped = 'Shipped',
  Delivered = 'Delivered',
  Cancelled = 'Cancelled',
}

export enum PaymentMethod {
  UpiQr = 'UpiQr',
}

export enum PaymentStatus {
  Pending = 'Pending',
  Paid = 'Paid',
  Failed = 'Failed',
  Refunded = 'Refunded',
}

export interface OrderItemDto {
  id: string;
  orderId: string;
  itemId: string;
  itemVariantId?: string | null;
  itemCode: string;
  itemName: string;
  variantSku?: string | null;
  variantName?: string | null;
  attributesJson?: string | null;
  imageUrl?: string | null;
  unitPrice: number;
  quantity: number;
  totalPrice: number;
}

export interface OrderDto {
  id: string;
  orderNumber: string;
  customerId?: string | null;
  customerName: string;
  customerEmail: string;
  customerPhone: string;
  shippingAddress: string;
  billingAddress?: string | null;
  orderNotes?: string | null;
  paymentMethod: PaymentMethod | string;
  paymentStatus: PaymentStatus | string;
  paymentReferenceNumber?: string | null;
  offlinePaymentNotes?: string | null;
  refundAmount?: number | null;
  refundReferenceNumber?: string | null;
  refundNotes?: string | null;
  refundedAt?: string | null;
  status: OrderStatus | string;
  trackingNumber?: string | null;
  shippingCarrier?: string | null;
  subtotalAmount: number;
  shippingFee: number;
  taxAmount: number;
  totalAmount: number;
  createdAt: string;
  updatedAt?: string | null;
  items: OrderItemDto[];
}

export interface OrderItemRequest {
  itemId: string;
  itemVariantId?: string | null;
  itemCode: string;
  itemName: string;
  variantSku?: string | null;
  variantName?: string | null;
  attributesJson?: string | null;
  imageUrl?: string | null;
  unitPrice: number;
  quantity: number;
}

export interface CreateOrderRequest {
  customerName: string;
  customerEmail: string;
  customerPhone: string;
  shippingAddress: string;
  billingAddress?: string | null;
  orderNotes?: string | null;
  paymentMethod: string;
  paymentReferenceNumber?: string | null;
  offlinePaymentNotes?: string | null;
  items: OrderItemRequest[];
}

export interface UpdateOrderStatusRequest {
  status: string;
  trackingNumber?: string | null;
  shippingCarrier?: string | null;
}

export interface UpdateOrderPaymentStatusRequest {
  paymentStatus: string;
  paymentReferenceNumber?: string | null;
  offlinePaymentNotes?: string | null;
}

export interface SubmitOrderPaymentDetailsRequest {
  paymentReferenceNumber: string;
  offlinePaymentNotes?: string | null;
}

export interface RecordOrderRefundRequest {
  refundAmount: number;
  refundReferenceNumber: string;
  refundNotes?: string | null;
}

export interface PaymentSettingsDto {
  id: string;
  upiId: string;
  payeeName?: string | null;
  qrCodeImageUrl?: string | null;
  instructions?: string | null;
  updatedAt: string;
}

export interface UpdatePaymentSettingsRequest {
  upiId: string;
  payeeName?: string | null;
  qrCodeImageUrl?: string | null;
  instructions?: string | null;
}


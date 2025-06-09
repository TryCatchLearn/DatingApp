export type Message = {
  id: string
  senderId: string
  senderDisplayName: string
  senderImageUrl: string
  recipientId: string
  recipientDisplayName: string
  recipientImageUrl: string
  content: string
  dateRead?: string
  messageSent: string
  currentUserSender?: boolean
}